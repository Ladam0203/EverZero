"use client"
import {useEffect, useState} from "react"
import {useRouter} from "next/navigation";
import {useAtom} from "jotai/index";
import {invoicesAtom} from "@/app/atoms/invoicesAtom";
import {authorize} from "@/app/server/auth/authorize";
import {getAllInvoices} from "@/app/server/invoice/getAllInvoices";
import AppNavbar from "@/app/components/AppNavbar";
import {FaExclamationTriangle, FaFileInvoiceDollar, FaPlus, FaSpinner} from "react-icons/fa";
import {format} from "date-fns";
import {InvoiceForm} from "@/app/components/InvoiceForm";
import {calculateEmission} from "@/app/server/emissionFactor/calculateEmission";

export default function Reports() {
    const router = useRouter();
    const [invoices, setInvoices] = useAtom(invoicesAtom)

    useEffect(() => {
        const doAuthorize = async () => {
            const response = await authorize();
            if (!response.authenticated) {
                throw new Error("Unauthorized");
            }
        };

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

        doAuthorize()
            .then(() => fetchInvoices())
            .catch((error) => {
                console.info("Authorization failed:", error.message);
                router.push("/login");
            });
    }, [invoices.loading, invoices.loaded, setInvoices]); // Now only depend on retryCount and loading/loaded states

    const createReport = async () => {
        const dto = invoices.invoices;

        console.log("Creating report with DTO:", dto);

        const response = await calculateEmission(dto);
        console.log(response);
    };

    return (
        <div className="min-h-screen bg-base-200">
            <AppNavbar/>

            <section className="p-8">

                <div className="flex justify-between items-center mb-8">
                    <h1 className="text-4xl font-bold">Reports</h1>
                    <button className="btn btn-primary" onClick={() => createReport()}>
                        <FaPlus className="mr-2"/> Create Report
                    </button>
                </div>

            </section>
        </div>
    )
}
