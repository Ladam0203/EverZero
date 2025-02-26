"use client"
import {useEffect, useState} from "react"
import {getAllInvoices} from "@/app/server/invoice/getAllInvoices"
import {useAtom} from "jotai"
import {invoicesAtom} from "@/app/atoms/invoicesAtom"
import {FaFileInvoiceDollar, FaSpinner, FaPlus, FaExclamationTriangle} from "react-icons/fa"
import {format} from "date-fns"
import {InvoiceForm} from "@/app/components/InvoiceForm"
import {useRouter} from "next/navigation"
import {authorize} from "@/app/server/auth/authorize";
import {postInvoice} from "@/app/server/invoice/postInvoice";
import AppNavbar from "@/app/components/AppNavbar";

export default function Invoices() {
    const router = useRouter();
    const [invoices, setInvoices] = useAtom(invoicesAtom)
    const [isModalOpen, setIsModalOpen] = useState(false)

    useEffect(() => {
        const doAuthorize = async () => {
            const response = await authorize();
            if (!response.authenticated) {
                throw new Error('Unauthorized');
            }
        };

        const fetchInvoices = async () => {
            if (invoices.loading || invoices.loaded) return;

            setInvoices((prev) => ({
                ...prev,
                loading: true,
            }));

            const result = await getAllInvoices();

            if (!result.success) {
                setInvoices((prev) => ({
                    ...prev,
                    loading: false,
                    error: result.message,
                    loaded: true,
                }));
                return;
            }

            setInvoices({
                invoices: result.data.length > 0 ? result.data : [],
                loading: false,
                loaded: true,
                error: null,
            });
            console.log("Invoices loaded:", result.data);
        };

        doAuthorize()
            .then(() => fetchInvoices())
            .catch((error) => {
                console.info('Authorization failed:', error.message);
                router.push('/login');
            });
    }, [invoices.loading, invoices.loaded]);

    const handleSubmit = async (newInvoice) => {
        console.log("New invoice to be sent to backend:", newInvoice)
        setIsModalOpen(false)
        const result = await postInvoice(newInvoice)
        console.log("Result of posting invoice:", result)
        if (!result.success) {
            // TODO: Display error (raise toast?)
            console.error("Failed to post invoice:", result.message)
            return
        }

        // Update the invoices list
        setInvoices((prevInvoices) => ({
            ...prevInvoices,
            invoices: [...prevInvoices.invoices, result.data],
        }))
    }

    return (
        <div className="min-h-screen bg-base-200">
            <AppNavbar/>

            <section className="p-8">

                <div className="flex justify-between items-center mb-8">
                    <h1 className="text-4xl font-bold">Invoices</h1>
                    <button className="btn btn-primary" onClick={() => setIsModalOpen(true)}>
                        <FaPlus className="mr-2"/> Add Invoice
                    </button>
                </div>

                {!invoices.loaded ? (
                    <div className="flex justify-center items-center">
                        <FaSpinner className="animate-spin text-4xl text-primary"/>
                    </div>
                ) : invoices.error ? (
                    <div className="flex justify-center items-center text-red-600">
                        <FaExclamationTriangle className="mr-2"/>
                        <span>{invoices.error}</span>
                    </div>
                ) : invoices.invoices.length === 0 ? (
                    <div className="flex justify-center items-center text-yellow-600">
                        <FaExclamationTriangle className="mr-2"/>
                        <span>No invoices found.</span>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {invoices.invoices.map((invoice) => (
                            <div key={invoice.id} className="card bg-base-100 shadow-xl">
                                <div className="card-body">
                                    <h2 className="card-title flex items-center">
                                        <FaFileInvoiceDollar className="text-primary mr-2"/>
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
            </section>

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
                    <InvoiceForm onSubmit={handleSubmit} onCancel={() => setIsModalOpen(false)}/>
                </div>
            </div>
        </div>
    )
}
