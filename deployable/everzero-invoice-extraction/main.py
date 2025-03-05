from fastapi import FastAPI, File, UploadFile, HTTPException
from typing import List

app = FastAPI()


@app.post("/extraction/extract")
async def extract_invoice(file: UploadFile = File(...)):
    if file.content_type != "application/pdf":
        raise HTTPException(status_code=400, detail="Invalid file type. Only PDF files are allowed.")

    # Placeholder for actual PDF parsing logic
    # Here you would typically read the PDF and extract the invoice data

    return {
        "subject": "MCA60043-MOGY-0424",
        "supplierName": "Community Utilities",
        "buyerName": "Mira Mogyin",
        "date": "2021-04-24",
        "lines": [
            {
                "description": "Heat Consumption",
                "quantity": 304,
                "unit": "kWh",
            },
            {
                "description": "Elec Consumption",
                "quantity": 137.82,
                "unit": "kWh",
            }
        ]
    }
