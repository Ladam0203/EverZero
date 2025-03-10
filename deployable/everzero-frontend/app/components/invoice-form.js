"use client"

import {useState, useRef} from "react"
import {getAllEmissionFactors} from "@/app/server/emission/getAllEmissionFactors"
import {useAtom} from "jotai"
import {emissionFactorsAtom} from "@/app/atoms/emissionFactorsAtom"
import ManualInvoiceForm from "@/app/components/manual-invoice-form";
import { FaUpload, FaFilePdf, FaArrowRight, FaEdit } from "react-icons/fa"

export const InvoiceForm = ({onSubmit, onCancel}) => {
    const [step, setStep] = useState("upload") // "upload" or "manual"
    const [isDragging, setIsDragging] = useState(false)
    const [file, setFile] = useState(null)
    const [isUploading, setIsUploading] = useState(false)
    const [extractedData, setExtractedData] = useState(null)
    const fileInputRef = useRef(null)

    const handleDragOver = (e) => {
        e.preventDefault()
        setIsDragging(true)
    }

    const handleDragLeave = () => {
        setIsDragging(false)
    }

    const handleDrop = (e) => {
        e.preventDefault()
        setIsDragging(false)

        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
            const droppedFile = e.dataTransfer.files[0]
            if (droppedFile.type === "application/pdf") {
                setFile(droppedFile)
           } else {
                alert("Please upload a PDF file")
            }
        }
    }

    const handleFileChange = (e) => {
        if (e.target.files && e.target.files[0]) {
            const selectedFile = e.target.files[0]
            if (selectedFile.type === "application/pdf") {
                setFile(selectedFile)
            } else {
                alert("Please upload a PDF file")
                e.target.value = null
            }
        }
    }

    const handleUpload = async () => {
        if (!file) return;

        setIsUploading(true);

        try {
            // Create FormData object and append the file
            const formData = new FormData();
            formData.append("file", file);

            // Make the POST request to the FastAPI endpoint
            const response = await fetch("api/extract", {
                method: "POST",
                body: formData,
            });

            // Check if the response is OK
            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message);
            }

            // Parse and log the response
            const data = await response.json();
            console.log("Response from endpoint:", data);

            // Set the extracted data
            setExtractedData(data.data);

            // Set the step to "manual"
            setStep("manual");
        } catch (error) {
            console.error("Error uploading file:", error.message);
            alert(`Upload failed: ${error.message}`);
        } finally {
            setIsUploading(false);
        }
    };

    const resetForm = () => {
        setStep("upload")
        setFile(null)
    }

    const handleOnSubmit = (invoice) => {
        // Call the onSubmit callback
        onSubmit(invoice)

        // Reset the form
        resetForm()
    }

    const handleOnCancel = () => {
        // Call the onCancel callback
        onCancel()

        resetForm()
    }

    if (step === "manual") {
        return (
            <ManualInvoiceForm onSubmit={onSubmit} onCancel={handleOnCancel} extractedData={extractedData}/>
        )
    }

    return (
        <div className="container mx-auto py-4 px-4">
            {!file ? (
                <div
                    className={`border-2 border-dashed rounded-lg p-10 text-center cursor-pointer transition-colors ${
                        isDragging ? "border-primary bg-primary/10" : "border-base-content/20 hover:border-primary/50"
                    }`}
                    onDragOver={handleDragOver}
                    onDragLeave={handleDragLeave}
                    onDrop={handleDrop}
                    onClick={() => fileInputRef.current?.click()}
                >
                    <input type="file" ref={fileInputRef} onChange={handleFileChange} accept=".pdf"
                           className="hidden"/>

                    <div className="flex flex-col items-center gap-4">
                        <div className="bg-primary/10 p-4 rounded-full">
                            <FaUpload className="h-8 w-8 text-primary"/>
                        </div>
                        <div>
                            <p className="font-medium mb-1">Drag and drop your PDF here</p>
                            <p className="text-sm text-base-content/60">or click to browse files</p>
                        </div>
                    </div>
                </div>
            ) : (
                <div className="border rounded-lg p-6">
                    <div className="flex items-center gap-4">
                        <div className="bg-primary/10 p-3 rounded-full">
                            <FaFilePdf className="h-6 w-6 text-primary"/>
                        </div>
                        <div className="flex-1">
                            <p className="font-medium truncate">{file.name}</p>
                            <p className="text-sm text-base-content/60">{(file.size / 1024).toFixed(2)} KB</p>
                        </div>
                    </div>
                </div>
            )}

            <div className="card-actions justify-between mt-6 pt-6 border-t">
                <button
                    className="btn"
                    onClick={handleOnCancel}
                >
                    Cancel
                </button>
                <div className="flex gap-4">
                    <button
                        className="btn btn-outline"
                        onClick={() => {
                            setStep("manual");
                            setFile(null)
                        }}
                    >
                        Upload Manually
                        {/** Manual Upload icon */}
                        <FaEdit className="ml-2"/>
                    </button>

                    <button className={`btn btn-primary ${!file ? "btn-disabled" : ""}`}
                            onClick={handleUpload}
                            disabled={isUploading}>
                        {isUploading ? "Uploading..." : "Upload Invoice"}
                        {!isUploading && <FaArrowRight className="ml-2"/>}
                    </button>
                </div>
            </div>
        </div>
    )
}

