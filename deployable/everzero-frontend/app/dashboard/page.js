"use client"
import { useEffect, useState } from "react"
import { getAllInvoices } from "@/app/server/invoice/getAllInvoices"
import { useAtom } from "jotai"
import { invoicesAtom } from "@/app/atoms/invoicesAtom"
import { FaFileInvoiceDollar, FaSpinner, FaPlus, FaExclamationTriangle } from "react-icons/fa"
import { format } from "date-fns"
import { InvoiceForm } from "@/app/components/InvoiceForm"

export default function Dashboard() {
    const [invoices, setInvoices] = useAtom(invoicesAtom)
    const [isModalOpen, setIsModalOpen] = useState(false)

    useEffect(() => {
        const fetchInvoices = async (retryCount = 0) => {
            if (invoices.loading || invoices.loaded) {
                return; // If invoices are already loading or already fetched, do nothing
            }

            setInvoices((prevInvoices) => ({
                ...prevInvoices,
                loading: true,
            }));

            const retryDelay = 1000 * Math.pow(2, retryCount); // Exponential backoff

            try {
                const result = await getAllInvoices();

                if (!result.success) {
                    throw new Error(result.message);
                }

                console.log("Fetched invoices:", result.data);

                setInvoices({
                    invoices: result.data.length > 0 ? result.data : [],
                    loading: false,
                    loaded: true,
                    error: null,
                });
            } catch (error) {
                console.error("Failed to fetch invoices:", error.message);

                if (retryCount < 3) {
                    setTimeout(() => fetchInvoices(retryCount + 1), retryDelay);
                } else {
                    setInvoices({
                        ...invoices,
                        loading: false,
                        error: "Failed to load invoices. Please try again later.",
                        loaded: true,
                    });
                }
            }
        };

        fetchInvoices();
    }, [invoices.loading, invoices.loaded, setInvoices]); // Now only depend on retryCount and loading/loaded states


    const handleSubmit = (newInvoice) => {
        console.log("New invoice to be sent to backend:", newInvoice)
        setIsModalOpen(false)
    }

    return (
        <div className="min-h-screen bg-base-200 p-8">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-4xl font-bold">Invoice Dashboard</h1>
                <button className="btn btn-primary" onClick={() => setIsModalOpen(true)}>
                    <FaPlus className="mr-2" /> Add Invoice
                </button>
            </div>

            {!invoices.loaded ? (
                <div className="flex justify-center items-center">
                    <FaSpinner className="animate-spin text-4xl text-primary" />
                </div>
            ) : invoices.error ? (
                <div className="flex justify-center items-center text-red-600">
                    <FaExclamationTriangle className="mr-2" />
                    <span>{invoices.error}</span>
                </div>
            ) : invoices.invoices.length === 0 ? (
                <div className="flex justify-center items-center text-yellow-600">
                    <FaExclamationTriangle className="mr-2" />
                    <span>No invoices found.</span>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {invoices.invoices.map((invoice) => (
                        <div key={invoice.id} className="card bg-base-100 shadow-xl">
                            <div className="card-body">
                                <h2 className="card-title flex items-center">
                                    <FaFileInvoiceDollar className="text-primary mr-2" />
                                    {invoice.subject}
                                </h2>
                                <p>
                                    <strong>Supplier:</strong> {invoice.supplierName}
                                </p>
                                <p>
                                    <strong>Buyer:</strong> {invoice.buyerName}
                                </p>
                                <p>
                                    <strong>Date:</strong> {format(new Date(invoice.date), "PPP")}
                                </p>
                                <div className="divider">Lines</div>
                                <table className="table table-compact w-full">
                                    <thead>
                                    <tr>
                                        <th>Description</th>
                                        <th>Quantity</th>
                                        <th>Unit</th>
                                    </tr>
                                    </thead>
                                    <tbody>
                                    {invoice.lines.map((line) => (
                                        <tr key={line.id}>
                                            <td>{line.description}</td>
                                            <td>{line.quantity}</td>
                                            <td>{line.unit}</td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {/* Add Invoice Modal */}
            <input
                type="checkbox"
                id="add-invoice-modal"
                className="modal-toggle"
                checked={isModalOpen}
                onChange={() => setIsModalOpen(!isModalOpen)}
            />
            <div className="modal">
                <div className="modal-box w-11/12 max-w-5xl">
                    <h3 className="font-bold text-lg mb-4">Add New Invoice</h3>
                    <InvoiceForm onSubmit={handleSubmit} onCancel={() => setIsModalOpen(false)} />
                </div>
            </div>
        </div>
    )
}
