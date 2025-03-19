import asyncio
from typing import List, Union
from fastapi import FastAPI, File, UploadFile, HTTPException

from pdf_extraction import extract_text_and_images, analyze_text_with_gpt

app = FastAPI()

MAX_FILE_SIZE = 10 * 1024 * 1024  # 10 MB
MAX_CONCURRENT = 10

@app.post("/extraction/extract", response_model=Union[dict, None])
async def extract_invoice(file: UploadFile = File(...)):
    if file.content_type != "application/pdf":
        raise HTTPException(status_code=400, detail="Invalid file type. Only PDF files are allowed.")
    if file.size > MAX_FILE_SIZE:
        raise HTTPException(status_code=400, detail="File too large. Max size is 10MB.")

    try:
        pdf_bytes = await file.read()
        full_text = extract_text_and_images(pdf_bytes)
        return analyze_text_with_gpt(full_text)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/extraction/extract/bulk", response_model=List[Union[dict, None]])
async def extract_invoices(files: List[UploadFile] = File(...)):
    semaphore = asyncio.Semaphore(MAX_CONCURRENT)
    tasks = [process_file(file, semaphore) for file in files]
    results = await asyncio.gather(*tasks, return_exceptions=True)

    structured_results = []
    for result in results:
        if isinstance(result, Exception):
            raise HTTPException(status_code=500, detail=str(result))
        structured_results.append(result)

    return structured_results

async def process_file(file: UploadFile, semaphore: asyncio.Semaphore):
    async with semaphore:
        return await extract_invoice(file)

