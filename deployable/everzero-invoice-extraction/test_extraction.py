from pdf_extraction import extract_text_and_images, analyze_text_with_gpt

# Test the extraction and GPT analysis with a sample PDF
def test_extract_and_analyze():
    # Load a sample PDF file as bytes
    with open("MCA60043-MOGY-0424.pdf", "rb") as pdf_file:
        pdf_bytes = pdf_file.read()

    try:
        # Step 1: Extract text
        extracted_text = extract_text_and_images(pdf_bytes)
        print("Extracted Text:")
        print(extracted_text)

        # Step 2: Analyze with GPT
        structured_data = analyze_text_with_gpt(extracted_text)
        print("\nStructured Data from GPT:")
        print(structured_data)

    except Exception as e:
        print(f"Error: {str(e)}")

if __name__ == "__main__":
    test_extract_and_analyze()