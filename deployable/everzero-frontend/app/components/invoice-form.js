"use client"

import {useState, useRef} from "react"
import {getAllEmissionFactors} from "@/app/server/emission/getAllEmissionFactors"
import {useAtom} from "jotai"
import {emissionFactorsAtom} from "@/app/atoms/emissionFactorsAtom"
import ManualInvoiceForm from "@/app/components/manual-invoice-form";
import { FaUpload, FaFilePdf, FaArrowRight, FaEdit, FaTimes } from "react-icons/fa"

export const InvoiceForm = ({onSubmit, onCancel}) => {
    const [step, setStep] = useState("upload")
    const [isDragging, setIsDragging] = useState(false)
    const [files, setFiles] = useState([])
    const [isUploading, setIsUploading] = useState(false)
    const [extractedData, setExtractedData] = useState(null)
    const fileInputRef = useRef(null)
    const MAX_FILE_SIZE = 10 * 1024 * 1024 // 10 MB in bytes
    const MAX_FILES = 10

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

        if (e.dataTransfer.files && e.dataTransfer.files.length) {
            processFiles(e.dataTransfer.files)
        }
    }

    const handleFileChange = (e) => {
        if (e.target.files && e.target.files.length) {
            processFiles(e.target.files)
            e.target.value = null // Reset input
        }
    }

    const processFiles = (newFiles) => {
        const validFiles = Array.from(newFiles).filter(file => {
            if (file.type !== "application/pdf") {
                alert(`"${file.name}" is not a PDF file and will be skipped`)
                return false
            }
            if (file.size > MAX_FILE_SIZE) {
                alert(`"${file.name}" exceeds 10 MB limit and will be skipped`)
                return false
            }
            return true
        })

        setFiles(prevFiles => {
            const combinedFiles = [...prevFiles, ...validFiles]
            if (combinedFiles.length > MAX_FILES) {
                alert(`Maximum of ${MAX_FILES} files allowed. Only first ${MAX_FILES} will be kept.`)
                return combinedFiles.slice(0, MAX_FILES)
            }
            return combinedFiles
        })
    }

    const removeFile = (indexToRemove) => {
        setFiles(prevFiles => prevFiles.filter((_, index) => index !== indexToRemove))
    }

    const handleUpload = async () => {
        if (!files.length) return

        setIsUploading(true)

        try {
            const formData = new FormData()
            files.forEach(file => {
                formData.append("files", file)
            })

            const response = await fetch("api/extract/bulk", {
                method: "POST",
                body: formData,
            })

            if (!response.ok) {
                const errorData = await response.json()
                throw new Error(errorData.message)
            }

            const data = await response.json()
            console.log("Response from bulk endpoint:", data)
            setExtractedData(data.data)
            setStep("manual")
        } catch (error) {
            console.error("Error uploading files:", error.message)
            alert(`Upload failed: ${error.message}`)
        } finally {
            setIsUploading(false)
        }
    }

    const resetForm = () => {
        setStep("upload")
        setFiles([])
    }

    const handleOnSubmit = (invoice) => {
        onSubmit(invoice)
        resetForm()
    }

    const handleOnCancel = () => {
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
            {!files.length ? (
                <div
                    className={`border-2 border-dashed rounded-lg p-10 text-center cursor-pointer transition-colors ${
                        isDragging ? "border-primary bg-primary/10" : "border-base-content/20 hover:border-primary/50"
                    }`}
                    onDragOver={handleDragOver}
                    onDragLeave={handleDragLeave}
                    onDrop={handleDrop}
                    onClick={() => fileInputRef.current?.click()}
                >
                    <input
                        type="file"
                        ref={fileInputRef}
                        onChange={handleFileChange}
                        accept=".pdf"
                        multiple
                        className="hidden"
                    />
                    <div className="flex flex-col items-center gap-4">
                        <div className="bg-primary/10 p-4 rounded-full">
                            <FaUpload className="h-8 w-8 text-primary"/>
                        </div>
                        <div>
                            <p className="font-medium mb-1">Drag and drop your PDFs here</p>
                            <p className="text-sm text-base-content/60">
                                or click to browse files (max {MAX_FILES}, 10 MB each)
                            </p>
                        </div>
                    </div>
                </div>
            ) : (
                <div className="border rounded-lg p-6 space-y-4">
                    {files.map((file, index) => (
                        <div key={index} className="flex items-center gap-4">
                            <div className="bg-primary/10 p-3 rounded-full">
                                <FaFilePdf className="h-6 w-6 text-primary"/>
                            </div>
                            <div className="flex-1">
                                <p className="font-medium truncate">{file.name}</p>
                                <p className="text-sm text-base-content/60">{(file.size / 1024).toFixed(2)} KB</p>
                            </div>
                            <button
                                className="btn btn-ghost btn-sm"
                                onClick={() => removeFile(index)}
                            >
                                <FaTimes className="h-4 w-4"/>
                            </button>
                        </div>
                    ))}
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
                            setStep("manual")
                            setFiles([])
                        }}
                    >
                        Upload Manually
                        <FaEdit className="ml-2"/>
                    </button>
                    <button
                        className={`btn btn-primary ${!files.length ? "btn-disabled" : ""}`}
                        onClick={handleUpload}
                        disabled={isUploading}
                    >
                        {isUploading ? "Uploading..." : `Upload ${files.length} Invoice${files.length > 1 ? "s" : ""}`}
                        {!isUploading && <FaArrowRight className="ml-2"/>}
                    </button>
                </div>
            </div>
        </div>
    )
}