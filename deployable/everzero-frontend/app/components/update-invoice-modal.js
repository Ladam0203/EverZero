import {useEffect} from "react";
import {emissionFactorsAtom} from "@/app/atoms/emissionFactorsAtom";
import {useAtom} from "jotai";
import {FaFileInvoiceDollar, FaMagic, FaPlus, FaTrash} from "react-icons/fa";

export default function UpdateInvoiceModal({invoice, onSubmit, onClose}) {
    const [emissionFactors, setEmissionFactors] = useAtom(emissionFactorsAtom); // Loaded by parent component

    useEffect(() => {
        console.log("Invoice to be updated:", invoice)
    }, [invoice]);

    return (
        <dialog id="update_invoice_modal" className="modal">
            <div className="modal-box w-11/12 max-w-5xl">
                <div className="modal-header flex items-center gap-2 mb-4">
                    <FaFileInvoiceDollar className="text-primary"/>
                    <h3 className="text-lg font-bold">Edit Invoice</h3>
                </div>
                {/* Conditional rendering (only show when invoice is not null) */}
                <form onSubmit={onSubmit} className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Subject</span>
                            </label>
                            <input
                                type="text"
                                name="subject"
                                value={invoice?.subject}
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
                                value={invoice?.supplierName}
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
                                value={invoice?.buyerName}
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
                                    invoice?.date ? new Date(invoice.date).toISOString().split("T")[0] : ""
                                }
                                className="input input-bordered"
                                required
                            />
                        </div>
                    </div>
                    <div className="divider">Invoice Lines</div>
                    {invoice && invoice.lines.map((line, index) => (
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
                                        className="input input-bordered"
                                        required
                                    />
                                </div>
                            </div>
                            <div className="form-control">
                                <button type="button" className="btn btn-error">
                                    <FaTrash className="mr-2"/> Remove Line
                                </button>
                            </div>
                        </div>
                    ))}
                    <button type="button" className="btn btn-secondary">
                        <FaPlus className="mr-2"/> Add Line
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
    )
}