"use client"

import { useState, useEffect } from "react"
import { FaPlus, FaTrash } from "react-icons/fa"
import { getAllEmissionFactors } from "@/app/server/emissionFactor/getAllEmissionFactors"
import {useAtom} from "jotai"
import {emissionFactorsAtom} from "@/app/atoms/emissionFactorsAtom";

export const InvoiceForm = ({ onSubmit, onCancel }) => {
    const testInvoice = {
        subject: "MCA60043-MOGY-0424",
        supplierName: "Community Utilities",
        buyerName: "Mira Mogyin",
        date: "2024-04-18",
        lines: [
            {
                description: "Heat Consumption",
                quantity: 304,
                unit: "kWh",
            },
            {
                description: "Elec Consumption",
                quantity: 137.82,
                unit: "kWh",
            },
        ],
    }

    const baseInvoice = {
        subject: "",
        supplierName: "",
        buyerName: "",
        date: new Date().toISOString().split("T")[0],
        lines: [],
    }

    const [invoice, setInvoice] = useState(process.env.NODE_ENV === "development" ? testInvoice : baseInvoice)
    const [emissionFactors, setEmissionFactors] = useAtom(emissionFactorsAtom)

    useEffect(() => {
        const fetchEmissionFactors = async (retryCount = 0) => {
            if (emissionFactors.loading || emissionFactors.loaded) {
                return; // If emission factors are already loading or already fetched, do nothing
            }

            setEmissionFactors((prevEmissionFactors) => ({
                ...prevEmissionFactors,
                loading: true,
            }))

            const retryDelay = 1000 * Math.pow(2, retryCount) // Exponential backoff

            try {
                const result = await getAllEmissionFactors()

                if (!result.success) {
                    throw new Error(result.message)
                }

                console.log("Fetched emission factors:", result.data)

                setEmissionFactors({
                    emissionFactors: result.data.length > 0 ? result.data : [],
                    loading: false,
                    loaded: true,
                    error: null,
                })
            } catch (error) {
                console.error("Failed to fetch emission factors:", error.message)

                if (retryCount < 3) {
                    setTimeout(() => fetchEmissionFactors(retryCount + 1), retryDelay)
                } else {
                    setEmissionFactors({
                        emissionFactors: [],
                        loading: false,
                        loaded: false,
                        error: error.message,
                    })
                }
            }
        }

        fetchEmissionFactors()
    }, [emissionFactors, setEmissionFactors])

    const handleInputChange = (e, index) => {
        const { name, value } = e.target
        if (name.startsWith("line")) {
            const [, field, lineIndex] = name.split("-")
            const updatedLines = [...invoice.lines]
            updatedLines[Number(lineIndex)] = { ...updatedLines[Number(lineIndex)], [field]: value }
            setInvoice({ ...invoice, lines: updatedLines })
        } else {
            setInvoice({ ...invoice, [name]: value })
        }
    }

    const addInvoiceLine = () => {
        setInvoice({
            ...invoice,
            lines: [
                ...invoice.lines,
                {
                    description: process.env.NODE_ENV === "development" ? "Test Line" : "",
                    quantity: process.env.NODE_ENV === "development" ? 1 : 0,
                    unit: process.env.NODE_ENV === "development" ? "kWh" : "",
                    category: "",
                    subCategories: {},
                    emissionFactorUnit: "",
                    emissionFactorId: "",
                },
            ],
        })
    }

    const removeInvoiceLine = (index) => {
        const updatedLines = invoice.lines.filter((_, i) => i !== index)
        setInvoice({ ...invoice, lines: updatedLines })
    }

    const handleSubmit = (e) => {
        e.preventDefault()
        onSubmit(invoice)
    }

    const handleEmissionFactorChange = (lineIndex, field, value) => {
        const updatedLines = [...invoice.lines]
        const currentLine = { ...updatedLines[lineIndex] }

        if (field === "category") {
            currentLine.category = value
            currentLine.subCategories = {}
            currentLine.emissionFactorId = ""
            currentLine.emissionFactorUnit = ""
        } else if (field.startsWith("subCategory-")) {
            const subCategoryKey = field.split("-")[1]
            currentLine.subCategories = {
                ...currentLine.subCategories,
                [subCategoryKey]: value,
            }
            currentLine.emissionFactorId = ""
            currentLine.emissionFactorUnit = ""
        } else if (field === "emissionFactorUnit") {
            currentLine.emissionFactorUnit = value
            // Find the matching emission factor and set its ID
            const matchingFactor = emissionFactors.emissionFactors.find(
                (ef) =>
                    ef.category === currentLine.category &&
                    Object.entries(ef.subCategories).every(([key, value]) => currentLine.subCategories[key] === value) &&
                    ef.unitEmissionFactors.some((uef) => uef.unit === value),
            )
            currentLine.emissionFactorId = matchingFactor ? matchingFactor.id : ""
        }

        updatedLines[lineIndex] = currentLine
        setInvoice({ ...invoice, lines: updatedLines })
    }

    return (
        <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="form-control">
                    <label className="label">
                        <span className="label-text">Subject</span>
                    </label>
                    <input
                        type="text"
                        name="subject"
                        value={invoice.subject}
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
                        value={invoice.supplierName}
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
                        value={invoice.buyerName}
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
                        value={invoice.date}
                        onChange={handleInputChange}
                        className="input input-bordered"
                        required
                    />
                </div>
            </div>
            <div className="divider">Invoice Lines</div>
            {invoice.lines.map((line, index) => (
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
                            <h4 className="text-lg font-semibold mb-2">Emission Factor</h4>
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
                                        {[...new Set(emissionFactors.emissionFactors.map((ef) => ef.category))].map((category) => (
                                            <option key={category} value={category}>
                                                {category}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                                {line.category && (
                                    <div className="space-y-4">
                                        {Object.keys(emissionFactors.emissionFactors.find((ef) => ef.category === line.category).subCategories).map(
                                            (subCategoryKey) => (
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
                                                        {[
                                                            ...new Set(
                                                                emissionFactors.emissionFactors
                                                                    .filter((ef) => ef.category === line.category)
                                                                    .map((ef) => ef.subCategories[subCategoryKey]),
                                                            ),
                                                        ].map((value) => (
                                                            <option key={value} value={value}>
                                                                {value}
                                                            </option>
                                                        ))}
                                                    </select>
                                                </div>
                                            ),
                                        )}
                                    </div>
                                )}
                                {line.category &&
                                    Object.keys(line.subCategories).length ===
                                    Object.keys(emissionFactors.emissionFactors.find((ef) => ef.category === line.category).subCategories).length && (
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
                                                {emissionFactors.emissionFactors
                                                    .find(
                                                        (ef) =>
                                                            ef.category === line.category &&
                                                            Object.entries(ef.subCategories).every(
                                                                ([key, value]) => line.subCategories[key] === value,
                                                            ),
                                                    )
                                                    ?.unitEmissionFactors.map((uef) => (
                                                        <option key={uef.unit} value={uef.unit}>
                                                            {uef.unit} ({uef.carbonEmissionKg} kg CO2e)
                                                        </option>
                                                    ))}
                                            </select>
                                        </div>
                                    )}
                                <input type="hidden" name={`line-emissionFactorId-${index}`} value={line.emissionFactorId || ""} />
                            </div>
                        </div>
                    )}
                    <div className="form-control">
                        <button type="button" className="btn btn-error" onClick={() => removeInvoiceLine(index)}>
                            <FaTrash className="mr-2" /> Remove Line
                        </button>
                    </div>
                </div>
            ))}
            <button type="button" className="btn btn-secondary" onClick={addInvoiceLine}>
                <FaPlus className="mr-2" /> Add Line
            </button>
            <div className="modal-action">
                <button type="submit" className="btn btn-primary">
                    Add Invoice
                </button>
                <button type="button" className="btn" onClick={onCancel}>
                    Cancel
                </button>
            </div>
        </form>
    )
}

