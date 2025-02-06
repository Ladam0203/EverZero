'use client';
import { useEffect } from "react";
import {getAllInvoices} from "@/app/server/invoice/getAllInvoices";

export default function Dashboard() {
    useEffect(() => {
        const fetchNotes = async () => {
            const invoicesResponse = await getAllInvoices();
            if (invoicesResponse.success) {
                console.log("Invoices response:", invoicesResponse.data);
            } else {
                console.error("Failed to fetch invoices:", invoicesResponse.message);
            }
        };

        fetchNotes();
    },);

    return (
        <div className="min-h-screen bg-base-200 flex items-center justify-center">
            <div className="card w-full max-w-md bg-base-100 shadow-xl p-6">
                <h2 className="card-title text-3xl font-bold text-center mb-6">Dashboard</h2>
                <p className="text-center">Welcome to the dashboard!</p>
            </div>
        </div>
    );
}