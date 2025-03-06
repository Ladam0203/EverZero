import os

from dotenv import load_dotenv
from fastapi import FastAPI, File, UploadFile, HTTPException
from pdf_extraction import extract_text_and_images, analyze_text_with_gpt  # Import both functions
app = FastAPI()

@app.post("/extraction/extract")
async def extract_invoice(file: UploadFile = File(...)):
    if file.content_type != "application/pdf":
        raise HTTPException(status_code=400, detail="Invalid file type. Only PDF files are allowed.")

    # Read the uploaded PDF file into memory
    pdf_bytes = await file.read()

    # Extract text and images
    try:
        full_text = extract_text_and_images(pdf_bytes)

        # Analyze the text with GPT to get structured data
        structured_data = analyze_text_with_gpt(full_text)

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing the PDF: {str(e)}")

    # Return the structured data from GPT
    return structured_data