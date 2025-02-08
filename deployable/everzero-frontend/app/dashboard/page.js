"use client"
import { useEffect, useState } from "react"
import { getAllInvoices } from "@/app/server/invoice/getAllInvoices"
import { useAtom } from "jotai"
import { invoicesAtom } from "@/app/atoms/invoicesAtom"
import { FaFileInvoiceDollar, FaSpinner, FaPlus } from "react-icons/fa"
import { format } from "date-fns"
import { InvoiceForm } from "@/app/components/InvoiceForm"

export default function Dashboard() {
    const [invoices, setInvoices] = useAtom(invoicesAtom)
    const [isModalOpen, setIsModalOpen] = useState(false)

    useEffect(() => {
        const fetchInvoices = async () => {
            if (invoices.loading || invoices.invoices.length > 0) {
                return
            }

            setInvoices({ ...invoices, loading: true })
            const result = await getAllInvoices()

            if (!result.success) {
                console.error("Failed to fetch invoices:", result.message)
                setInvoices({ ...invoices, loading: false })
                return
            }

            console.log("Fetched invoices:", result.data)
            setInvoices({ invoices: result.data, loading: false })
        }

        fetchInvoices()
    }, [invoices, setInvoices])

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
            {invoices.loading ? (
                <div className="flex justify-center items-center">
                    <FaSpinner className="animate-spin text-4xl text-primary" />
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

