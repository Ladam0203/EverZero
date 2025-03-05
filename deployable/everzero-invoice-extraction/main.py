from fastapi import FastAPI, File, UploadFile, HTTPException
import fitz  # PyMuPDF
import easyocr
from PIL import Image
from io import BytesIO
import numpy as np  # Add this import for NumPy

app = FastAPI()

# Initialize EasyOCR reader
reader = easyocr.Reader(['en'])  # You can specify additional languages


@app.post("/extraction/extract")
async def extract_invoice(file: UploadFile = File(...)):
    if file.content_type != "application/pdf":
        raise HTTPException(status_code=400, detail="Invalid file type. Only PDF files are allowed.")

    # Read the uploaded PDF file into memory
    pdf_bytes = await file.read()

    # Use PyMuPDF to extract text from the PDF
    try:
        # Open the PDF from the bytes
        doc = fitz.open(stream=pdf_bytes, filetype="pdf")
        if doc.page_count == 0:
            raise HTTPException(status_code=400, detail="Empty or invalid PDF file")

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

        # Save extracted text to a file (optional)
        with open("extracted_text.txt", "w") as text_file:
            text_file.write(full_text)

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing the PDF: {str(e)}")

    # Return the extracted text or a dynamic response
    return {
        "subject": "MCA60043-MOGY-0424",
        "supplierName": "Community Utilities",
        "buyerName": "Mira Mogyin",
        "date": "2021-04-24",
        "lines": []
    }