import asyncio
from typing import List
from fastapi import FastAPI, File, UploadFile, HTTPException

from pdf_extraction import extract_text_and_images, analyze_text_with_gpt

app = FastAPI()

MAX_FILE_SIZE = 10 * 1024 * 1024  # 10 MB
MAX_CONCURRENT = 10


async def extract_invoice(file: UploadFile):
    if file.content_type != "application/pdf":
        raise HTTPException(status_code=400, detail="Invalid file type. Only PDF files are allowed.")
    if file.size > MAX_FILE_SIZE:
        raise HTTPException(status_code=400, detail="File too large. Max size is 10MB.")

    pdf_bytes = await file.read()
    full_text = extract_text_and_images(pdf_bytes)
    structured_data = analyze_text_with_gpt(full_text)

    return structured_data


async def process_file(file: UploadFile, semaphore: asyncio.Semaphore):
    async with semaphore:
        return await extract_invoice(file)


@app.post("/extraction/extract/bulk")
async def extract_invoices(files: List[UploadFile] = File(...)):
    semaphore = asyncio.Semaphore(MAX_CONCURRENT)
    tasks = [process_file(file, semaphore) for file in files]
    results = await asyncio.gather(*tasks, return_exceptions=True)

    response = []
    for i, result in enumerate(results):
        if isinstance(result, Exception):
            response.append({"file": files[i].filename, "error": str(result)})
        else:
            response.append({"file": files[i].filename, "data": result})

    return response