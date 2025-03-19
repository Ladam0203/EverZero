"use client"

import {useState, useEffect} from "react"
import {FaPlus, FaTrash, FaMagic, FaArrowLeft, FaArrowRight} from "react-icons/fa"
import {useAtom} from "jotai"
import {emissionFactorsAtom} from "@/app/atoms/emissionFactorsAtom"

export default function ManualInvoiceForm({onSubmit, onCancel, extractedData}) {
    const baseInvoice = {
        subject: "",
        supplierName: "",
        buyerName: "",
        date: new Date().toISOString().split("T")[0],
        lines: [],
    }

    const [invoices, setInvoices] = useState([baseInvoice])
    const [currentIndex, setCurrentIndex] = useState(0)
    const [emissionFactors] = useAtom(emissionFactorsAtom)

    useEffect(() => {
        if (extractedData && Array.isArray(extractedData) && extractedData.length > 0) {
            const formattedInvoices = extractedData.map(data => ({
                subject: data.subject || "",
                supplierName: data.supplierName || "",
                buyerName: data.buyerName || "",
                date: data.date || new Date().toISOString().split("T")[0],
                lines: data.lines?.map(line => ({
                    description: line.description || "",
                    quantity: line.quantity || 0,
                    unit: line.unit || "",
                    category: "",
                    subCategories: {},
                    emissionFactorUnit: "",
                    emissionFactorId: "",
                })) || [],
            }))
            setInvoices(formattedInvoices)
        }
    }, [extractedData])

    const currentInvoice = invoices[currentIndex]

    const handleInputChange = (e, lineIndex) => {
        const {name, value} = e.target
        const updatedInvoices = [...invoices]

        if (name.startsWith("line")) {
            const [, field, index] = name.split("-")
            const updatedLines = [...currentInvoice.lines]
            updatedLines[Number(index)] = {...updatedLines[Number(index)], [field]: value}
            updatedInvoices[currentIndex] = {...currentInvoice, lines: updatedLines}
        } else {
            updatedInvoices[currentIndex] = {...currentInvoice, [name]: value}
        }
        setInvoices(updatedInvoices)
    }

    const updateEmissionFactorId = (index, supplierName, description, unit) => {
        getEmissionFactorIdSuggestion(supplierName, description, unit).then((data) => {
            if (data.success) {
                const emissionFactorId = data.data.emissionFactorId
                const updatedInvoices = [...invoices]
                const updatedLine = {...currentInvoice.lines[index]}
                const emissionFactor = emissionFactors.emissionFactors.find(ef => ef.id === emissionFactorId)

                updatedLine.category = emissionFactor.category
                updatedLine.subCategories = emissionFactor.subCategories
                updatedLine.emissionFactorUnit = emissionFactor.unit
                updatedLine.emissionFactorId = emissionFactorId

                const updatedLines = [...currentInvoice.lines]
                updatedLines[index] = updatedLine
                updatedInvoices[currentIndex] = {...currentInvoice, lines: updatedLines}
                setInvoices(updatedInvoices)
            }
        })
    }

    const getEmissionFactorIdSuggestion = async (supplierName, invoiceLineDescription, unit) => {
        try {
            const response = await fetch(`/api/invoices/suggestions/emission-factor-id?supplierName=${supplierName}&invoiceLineDescription=${invoiceLineDescription}&unit=${unit}`)
            return await response.json()
        } catch (error) {
            console.error("Failed to get emission factor ID suggestion", error)
            return {success: false, message: "Failed to get emission factor ID suggestion"}
        }
    }

    const addInvoiceLine = () => {
        const updatedInvoices = [...invoices]
        updatedInvoices[currentIndex] = {
            ...currentInvoice,
            lines: [...currentInvoice.lines, {
                description: process.env.NODE_ENV === "development" ? "Test Line" : "",
                quantity: process.env.NODE_ENV === "development" ? 1 : 0,
                unit: process.env.NODE_ENV === "development" ? "kWh" : "",
                category: "",
                subCategories: {},
                emissionFactorUnit: "",
                emissionFactorId: "",
            }]
        }
        setInvoices(updatedInvoices)
    }

    const removeInvoiceLine = (index) => {
        const updatedInvoices = [...invoices]
        updatedInvoices[currentIndex] = {
            ...currentInvoice,
            lines: currentInvoice.lines.filter((_, i) => i !== index)
        }
        setInvoices(updatedInvoices)
    }

    const handleEmissionFactorChange = (lineIndex, field, value) => {
        const updatedInvoices = [...invoices]
        const currentLine = {...currentInvoice.lines[lineIndex]}

        if (field === "category") {
            currentLine.category = value
            currentLine.subCategories = {}
            currentLine.emissionFactorId = ""
            currentLine.emissionFactorUnit = ""

            const categoryFactors = emissionFactors.emissionFactors.filter(ef => ef.category === value)
            if (categoryFactors[0] && currentLine) {
                Object.keys(categoryFactors[0].subCategories).forEach(subCategoryKey => {
                    const subCategoryOptions = [...new Set(categoryFactors.map(ef => ef.subCategories[subCategoryKey]))]
                    if (subCategoryOptions.length === 1) {
                        currentLine.subCategories[subCategoryKey] = subCategoryOptions[0]
                    }
                })

                if (Object.keys(currentLine.subCategories).length === Object.keys(categoryFactors[0].subCategories).length) {
                    const unitOptions = getEmissionFactorUnitOptions(value, currentLine.subCategories)
                    if (unitOptions.length === 1) {
                        currentLine.emissionFactorUnit = unitOptions[0].unit
                        currentLine.emissionFactorId = unitOptions[0].id
                    }
                }
            }
        } else if (field.startsWith("subCategory-")) {
            const subCategoryKey = field.split("-")[1]
            currentLine.subCategories = {
                ...currentLine.subCategories,
                [subCategoryKey]: value,
            }
            currentLine.emissionFactorId = ""
            currentLine.emissionFactorUnit = ""

            const categoryFactors = emissionFactors.emissionFactors.find(ef => ef.category === currentLine.category)
            if (
                categoryFactors &&
                Object.keys(currentLine.subCategories).length === Object.keys(categoryFactors.subCategories).length
            ) {
                const unitOptions = getEmissionFactorUnitOptions(currentLine.category, currentLine.subCategories)
                if (unitOptions.length === 1) {
                    currentLine.emissionFactorUnit = unitOptions[0].unit
                    currentLine.emissionFactorId = unitOptions[0].id
                }
            }
        } else if (field === "emissionFactorUnit") {
            currentLine.emissionFactorUnit = value
            const matchingFactor = getEmissionFactorUnitOptions(currentLine.category, currentLine.subCategories)
                .find(uef => uef.unit === value)
            currentLine.emissionFactorId = matchingFactor ? matchingFactor.id : ""
        }

        const updatedLines = [...currentInvoice.lines]
        updatedLines[lineIndex] = currentLine
        updatedInvoices[currentIndex] = {...currentInvoice, lines: updatedLines}
        setInvoices(updatedInvoices)
    }

    const getSubCategoryOptions = (category, subCategoryKey) => {
        return [...new Set(
            emissionFactors.emissionFactors
                .filter(ef => ef.category === category)
                .map(ef => ef.subCategories[subCategoryKey])
        )]
    }

    const getEmissionFactorUnitOptions = (category, subCategories) => {
        return emissionFactors.emissionFactors
            .filter(ef =>
                ef.category === category &&
                Object.entries(subCategories).every(([key, value]) => ef.subCategories[key] === value)
            )
            .map(ef => ({
                unit: ef.unit,
                carbonEmissionKg: ef.carbonEmissionKg,
                id: ef.id
            }))
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        try {
            const response = await fetch("/api/invoices/bulk", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify(invoices),
            })
            if (!response.ok) {
                const errorData = await response.json()
                throw new Error(errorData.message)
            }
            onSubmit(invoices)
        } catch (error) {
            console.error("Error submitting invoices:", error.message)
            alert(`Submission failed: ${error.message}`)
        }
    }

    const goToPrevious = () => setCurrentIndex(prev => Math.max(0, prev - 1))
    const goToNext = () => setCurrentIndex(prev => Math.min(invoices.length - 1, prev + 1))

    return (
        <form onSubmit={handleSubmit} className="space-y-6">
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-bold">
                    Invoice {currentIndex + 1} of {invoices.length}
                </h2>
                <div className="flex gap-2">
                    <button
                        type="button"
                        className="btn btn-sm"
                        onClick={goToPrevious}
                        disabled={currentIndex === 0}
                    >
                        <FaArrowLeft/>
                    </button>
                    <button
                        type="button"
                        className="btn btn-sm"
                        onClick={goToNext}
                        disabled={currentIndex === invoices.length - 1}
                    >
                        <FaArrowRight/>
                    </button>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="form-control">
                    <label className="label">
                        <span className="label-text">Subject</span>
                    </label>
                    <input
                        type="text"
                        name="subject"
                        value={currentInvoice.subject}
                        onChange={handleInputChange}
                        className="input input-bordered"
                        required
                    />
                </div>
                <div className="form-control">
                    <label className="label">
                        <span className="label-text">Supplier Name</span>
                    </label>
                    <input
                        type="text"
                        name="supplierName"
                        value={currentInvoice.supplierName}
                        onChange={handleInputChange}
                        className="input input-bordered"
                        required
                    />
                </div>
                <div className="form-control">
                    <label className="label">
                        <span className="label-text">Buyer Name</span>
                    </label>
                    <input
                        type="text"
                        name="buyerName"
                        value={currentInvoice.buyerName}
                        onChange={handleInputChange}
                        className="input input-bordered"
                        required
                    />
                </div>
                <div className="form-control">
                    <label className="label">
                        <span className="label-text">Date</span>
                    </label>
                    <input
                        type="date"
                        name="date"
                        value={currentInvoice.date}
                        onChange={handleInputChange}
                        className="input input-bordered"
                        required
                    />
                </div>
            </div>

            <div className="divider">Invoice Lines</div>
            {currentInvoice.lines.map((line, index) => (
                <div key={index} className="space-y-4 p-4 bg-base-200 rounded-lg mb-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Description</span>
                            </label>
                            <input
                                type="text"
                                name={`line-description-${index}`}
                                value={line.description}
                                onChange={(e) => handleInputChange(e, index)}
                                className="input input-bordered"
                                required
                            />
                        </div>
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Quantity</span>
                            </label>
                            <input
                                type="number"
                                name={`line-quantity-${index}`}
                                value={line.quantity}
                                onChange={(e) => handleInputChange(e, index)}
                                className="input input-bordered"
                                required
                            />
                        </div>
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Unit</span>
                            </label>
                            <input
                                type="text"
                                name={`line-unit-${index}`}
                                value={line.unit}
                                onChange={(e) => handleInputChange(e, index)}
                                className="input input-bordered"
                                required
                            />
                        </div>
                    </div>
                    {emissionFactors.loaded && emissionFactors.emissionFactors.length > 0 && (
                        <div className="bg-base-100 p-4 rounded-lg">
                            <div className="flex items-center justify-between">
                                <h2 className="text-lg font-bold">Emission Factor</h2>
                                <button
                                    type="button"
                                    className="btn btn-sm btn-ghost"
                                    onClick={() =>
                                        updateEmissionFactorId(index, currentInvoice.supplierName, line.description, line.unit)
                                    }
                                >
                                    <FaMagic/>
                                </button>
                            </div>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text">Category</span>
                                    </label>
                                    <select
                                        className="select select-bordered"
                                        value={line.category || ""}
                                        onChange={(e) => handleEmissionFactorChange(index, "category", e.target.value)}
                                    >
                                        <option value="">Select category</option>
                                        {[...new Set(emissionFactors.emissionFactors.map(ef => ef.category))].map(category => (
                                            <option key={category} value={category}>
                                                {category}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                                {line.category && (
                                    <div className="space-y-4">
                                        {Object.keys(
                                            emissionFactors.emissionFactors.find(ef => ef.category === line.category).subCategories
                                        ).map(subCategoryKey => {
                                            const options = getSubCategoryOptions(line.category, subCategoryKey)
                                            if (options.length === 1 && !line.subCategories[subCategoryKey]) {
                                                handleEmissionFactorChange(index, `subCategory-${subCategoryKey}`, options[0])
                                            }
                                            return (
                                                <div key={subCategoryKey} className="form-control">
                                                    <label className="label">
                                                        <span className="label-text">{subCategoryKey}</span>
                                                    </label>
                                                    <select
                                                        className="select select-bordered"
                                                        value={line.subCategories[subCategoryKey] || ""}
                                                        onChange={(e) =>
                                                            handleEmissionFactorChange(index, `subCategory-${subCategoryKey}`, e.target.value)
                                                        }
                                                    >
                                                        <option value="">Select {subCategoryKey}</option>
                                                        {options.map(value => (
                                                            <option key={value} value={value}>
                                                                {value}
                                                            </option>
                                                        ))}
                                                    </select>
                                                </div>
                                            )
                                        })}
                                    </div>
                                )}
                                {line.category &&
                                    Object.keys(line.subCategories).length ===
                                    Object.keys(
                                        emissionFactors.emissionFactors.find(ef => ef.category === line.category).subCategories
                                    ).length && (
                                        <div className="form-control">
                                            <label className="label">
                                                <span className="label-text">Emission Factor Unit</span>
                                            </label>
                                            <select
                                                className="select select-bordered"
                                                value={line.emissionFactorUnit || ""}
                                                onChange={(e) => handleEmissionFactorChange(index, "emissionFactorUnit", e.target.value)}
                                            >
                                                <option value="">Select unit</option>
                                                {getEmissionFactorUnitOptions(line.category, line.subCategories).map(uef => (
                                                    <option key={uef.unit} value={uef.unit}>
                                                        {uef.unit} ({uef.carbonEmissionKg} kg CO2e)
                                                    </option>
                                                ))}
                                            </select>
                                        </div>
                                    )}
                                <input
                                    type="hidden"
                                    name={`line-emissionFactorId-${index}`}
                                    value={line.emissionFactorId || ""}
                                />
                            </div>
                        </div>
                    )}
                    <div className="form-control">
                        <button type="button" className="btn btn-error" onClick={() => removeInvoiceLine(index)}>
                            <FaTrash className="mr-2"/> Remove Line
                        </button>
                    </div>
                </div>
            ))}

            <button type="button" className="btn btn-secondary" onClick={addInvoiceLine}>
                <FaPlus className="mr-2"/> Add Line
            </button>

            <div className="modal-action justify-between">
                <button type="button" className="btn" onClick={onCancel}>
                    Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                    Save All Invoices
                </button>
            </div>
        </form>
    )
}