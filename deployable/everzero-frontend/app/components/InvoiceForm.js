"use client"

import { useState } from "react"
import { FaPlus, FaTrash } from "react-icons/fa"

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
            lines: [...invoice.lines, { description: process.env.NODE_ENV === "development" ? "Test Line" : "",
                quantity: process.env.NODE_ENV === "development" ? 1 : 0,
                unit: process.env.NODE_ENV === "development" ? "kWh" : ""
            }],
        })
    }

    const removeInvoiceLine = (index) => {
        const updatedLines = invoice.lines.filter((_, i) => i !== index)
        setInvoice({ ...invoice, lines: updatedLines })
    }

    const handleSubmit = (e) => {
        // TODO: Add validation for the invoice
        e.preventDefault()
        onSubmit(invoice)
    }

    return (
        <form onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
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
                <div key={index} className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4 items-end">
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
                    <div className="form-control">
                        <button type="button" className="btn btn-error" onClick={() => removeInvoiceLine(index)}>
                            <FaTrash className="mr-2" /> Remove
                        </button>
                    </div>
                </div>
            ))}
            <button type="button" className="btn btn-secondary mb-4" onClick={addInvoiceLine}>
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

