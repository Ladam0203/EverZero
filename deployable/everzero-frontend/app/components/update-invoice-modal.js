import { useEffect } from "react";

export default function UpdateInvoiceModal({ invoice, onSubmit, onClose }) {

    useEffect(() => {
        console.log("Invoice to be updated:", invoice)
    }, [invoice]);

    return (
        <dialog id="update_invoice_modal" className="modal">
            <div className="modal-box">
                <h3 className="text-lg font-bold">Hello!</h3>
                <p className="py-4">Press ESC key or click the button below to close</p>
                <div className="modal-action">
                    <form method="dialog">
                        <button className="btn">Close</button>
                    </form>
                </div>
            </div>
        </dialog>
    )
}