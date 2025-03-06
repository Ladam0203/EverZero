import os

import fitz  # PyMuPDF
import easyocr
from PIL import Image
from io import BytesIO
import numpy as np
from dotenv import load_dotenv
from openai import OpenAI

# Load environment variables
load_dotenv()

# Initialize EasyOCR reader
reader = easyocr.Reader(['en'])  # Specify additional languages as needed

api_key = os.getenv("OPENAI_API_KEY")
if not api_key:
    raise ValueError("OPENAI_API_KEY environment variable not set")
client = OpenAI(api_key=api_key)


def extract_text_and_images(pdf_bytes):
    """
    Extract text and images (with OCR) from a PDF file provided as bytes.

    Args:
        pdf_bytes (bytes): The PDF file content as bytes.

    Returns:
        str: The extracted text from the PDF and OCR'd images.

    Raises:
        Exception: If there's an error processing the PDF.
    """
    # Open the PDF from the bytes
    doc = fitz.open(stream=pdf_bytes, filetype="pdf")
    if doc.page_count == 0:
        raise ValueError("Empty or invalid PDF file")

    # Initialize a string to hold all extracted text
    full_text = ""

    # Loop through each page and extract text
    for page_num in range(len(doc)):
        page = doc.load_page(page_num)
        full_text += page.get_text()

        # Extract images and apply OCR
        image_list = page.get_images(full=True)
        for img_index, img in enumerate(image_list):
            try:
                xref = img[0]
                base_image = doc.extract_image(xref)
                image_bytes = base_image["image"]
                image = Image.open(BytesIO(image_bytes)).convert("RGB")  # Convert to RGB
            except Exception as e:
                full_text += f"\n[Error extracting image {img_index + 1}]: {str(e)}"
                continue

            # Apply OCR to the image by converting to NumPy array
            try:
                image_np = np.array(image)  # Convert PIL Image to NumPy array
                ocr_text = reader.readtext(image_np)  # Pass NumPy array to EasyOCR
                if ocr_text:
                    for detection in ocr_text:
                        full_text += f"\n[OCR from image {img_index + 1}]: {detection[1]}"
            except Exception as e:
                full_text += f"\n[OCR error on image {img_index + 1}]: {str(e)}"

    return full_text


def analyze_text_with_gpt(full_text):
    """
    Analyze the extracted text using GPT and return structured data.

    Args:
        full_text (str): The text extracted from the PDF.

    Returns:
        dict: Structured data extracted by GPT (e.g., subject, supplierName, etc.).

    Raises:
        Exception: If the GPT API call fails.
    """
    # Define the function schema for GPT to follow
    function_schema = {
        "name": "extract_invoice_data",
        "description": "Extract structured invoice data from text",
        "parameters": {
            "type": "object",
            "properties": {
                "subject": {"type": "string", "description": "The subject or reference number of the invoice"},
                "supplierName": {"type": "string", "description": "The name of the supplier"},
                "buyerName": {"type": "string", "description": "The name of the buyer"},
                "date": {"type": "string", "description": "The date of the invoice (YYYY-MM-DD)"},
                "lines": {
                    "description": "List of invoice line items (can be empty)",
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "description": {"type": "string", "description": "Description of the line item"},
                            "unit": {"type": "string",
                                     "description": "Unit of measurement (e.g., 'pcs', 'kWh', 'hours')"},
                            "quantity": {"type": "number", "description": "Quantity of the item"}
                        },
                        "required": ["description", "unit", "quantity"]
                    }
                }
            },
            "required": ["subject", "supplierName", "buyerName", "date", "lines"]
        }
    }

    # Prompt GPT to analyze the text and return structured data
    try:
        response = client.chat.completions.create(
            model="gpt-4o",  # Use an appropriate model (e.g., gpt-4o, gpt-3.5-turbo)
            messages=[
                {"role": "system",
                 "content": "You are an expert at extracting structured data from unstructured UK invoice text. "
                            "You are extracting information from UK invoice texts with the aim to later produce a carbon emission report."
                            "You know that it is important to find the actual supplier, instead of the intermediary company, take a good look on who is the invoice originally from (often only denoted in the logo)."
                            "You know you only have to extract invoice lines that are then can be used in a carbon emission calculation, if it's something like irrelevant costs (like delivery costs) you can ignore them."
                            "Additionally, the casing of the text is not always consistent, so you need to be able to handle that, convert the casing so that it would make sense."
                 },
                {"role": "user", "content": f"Extract the following invoice details from this text:\n\n{full_text}"}
            ],
            functions=[function_schema],
            function_call={"name": "extract_invoice_data"}
        )

        # Extract the function call result
        function_call = response.choices[0].message.function_call
        if function_call and function_call.name == "extract_invoice_data":
            import json
            return json.loads(function_call.arguments)
        else:
            raise ValueError("GPT did not return the expected function call")

    except Exception as e:
        raise Exception(f"Error analyzing text with GPT: {str(e)}")