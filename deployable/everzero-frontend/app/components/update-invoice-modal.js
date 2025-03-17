import { useEffect, useState } from "react";
import { emissionFactorsAtom } from "@/app/atoms/emissionFactorsAtom";
import { useAtom } from "jotai";
import { FaFileInvoiceDollar, FaPlus, FaTrash } from "react-icons/fa";

export default function UpdateInvoiceModal({ invoice, onSubmit, onClose }) {
    const [emissionFactors, setEmissionFactors] = useAtom(emissionFactorsAtom);
    const [formData, setFormData] = useState(null); // Local state for form data

    // Initialize formData when invoice prop changes
    useEffect(() => {
        if (invoice) {
            setFormData({
                ...invoice,
                lines: invoice.lines ? [...invoice.lines] : [], // Deep copy of lines array
            });
            console.log("Invoice to be updated:", invoice);
        }
    }, [invoice]);

    // Handle changes to text inputs
    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData((prev) => ({
            ...prev,
            [name]: value,
        }));
    };

    // Handle changes to invoice line fields
    const handleLineChange = (index, field, value) => {
        setFormData((prev) => {
            const updatedLines = [...prev.lines];
            updatedLines[index] = {
                ...updatedLines[index],
                [field]: value,
            };
            return { ...prev, lines: updatedLines };
        });
    };

    // Add a new empty line
    const addLine = () => {
        setFormData((prev) => ({
            ...prev,
            lines: [
                ...prev.lines,
                { description: "", quantity: 0, unit: "" }, // Default empty line
            ],
        }));
    };

    // Remove a line by index
    const removeLine = (index) => {
        setFormData((prev) => ({
            ...prev,
            lines: prev.lines.filter((_, i) => i !== index),
        }));
    };

    // Handle form submission
    const handleSubmit = (e) => {
        e.preventDefault();
        onSubmit(formData); // Pass the updated data to the parent
    };

    if (!formData) return <dialog id="update_invoice_modal" className="modal"></dialog>;

    return (
        <dialog id="update_invoice_modal" className="modal">
            <div className="modal-box w-11/12 max-w-5xl">
                <div className="modal-header flex items-center gap-2 mb-4">
                    <FaFileInvoiceDollar className="text-primary" />
                    <h3 className="text-lg font-bold">Edit Invoice</h3>
                </div>
                <form onSubmit={handleSubmit} className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Subject</span>
                            </label>
                            <input
                                type="text"
                                name="subject"
                                value={formData.subject || ""}
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
                                value={formData.supplierName || ""}
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
                                value={formData.buyerName || ""}
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
                                value={
                                    formData.date
                                        ? new Date(formData.date).toISOString().split("T")[0]
                                        : ""
                                }
                                onChange={handleInputChange}
                                className="input input-bordered"
                                required
                            />
                        </div>
                    </div>
                    <div className="divider">Invoice Lines</div>
                    {formData.lines.map((line, index) => (
                        <div key={index} className="space-y-4 p-4 bg-base-200 rounded-lg mb-4">
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text">Description</span>
                                    </label>
                                    <input
                                        type="text"
                                        value={line.description || ""}
                                        onChange={(e) =>
                                            handleLineChange(index, "description", e.target.value)
                                        }
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
                                        value={line.quantity || 0}
                                        onChange={(e) =>
                                            handleLineChange(index, "quantity", e.target.value)
                                        }
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
                                        value={line.unit || ""}
                                        onChange={(e) =>
                                            handleLineChange(index, "unit", e.target.value)
                                        }
                                        className="input input-bordered"
                                        required
                                    />
                                </div>
                            </div>
                            <div className="form-control">
                                <button
                                    type="button"
                                    className="btn btn-error"
                                    onClick={() => removeLine(index)}
                                >
                                    <FaTrash className="mr-2" /> Remove Line
                                </button>
                            </div>
                        </div>
                    ))}
                    <button type="button" className="btn btn-secondary" onClick={addLine}>
                        <FaPlus className="mr-2" /> Add Line
                    </button>
                    <div className="modal-action justify-between">
                        <button type="button" className="btn" onClick={onClose}>
                            Cancel
                        </button>
                        <button type="submit" className="btn btn-primary">
                            Update Invoice
                        </button>
                    </div>
                </form>
            </div>
        </dialog>
    );
}