'use client';

import AppNavbar from "@/app/components/AppNavbar";
import { useRouter } from "next/navigation";
import { useAtom } from "jotai";
import { invoicesAtom } from "@/app/atoms/invoicesAtom";
import { authorize } from "@/app/server/auth/authorize";
import { useEffect, useState } from "react";
import { Pie } from 'react-chartjs-2';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';

// Registering the required chart elements
ChartJS.register(ArcElement, Tooltip, Legend);

export default function Dashboard() {
    const router = useRouter();
    const [invoices, setInvoices] = useAtom(invoicesAtom);
    const [calculation, setCaculation] = useState(null);

    const chartData = {
        labels: calculation?.scopes?.map((scope) => scope.scope),
        datasets: [
            {
                data: calculation?.scopes?.map((scope) => scope.emission),
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    ],
                hoverOffset: 4,
            },
        ],
    };

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

            const result = await fetch('api/invoices').then((res) => res.json());

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

        const fetchCalculation = async () => {
            if (invoices.loading || !invoices.loaded) return;

            const dto = invoices.invoices;
            console.log("Calculation DTO:", dto);

            const result = await fetch('api/calculate', {
                method: 'POST',
                body: JSON.stringify(dto),
            }).then((res) => res.json());

            if (!result.success) {
                setCaculation(null);
                return;
            }

            console.log("Calculation result:", result.data);
            setCaculation(result.data);
        };

        doAuthorize()
            .then(() => fetchInvoices())
            .then(() => fetchCalculation())
            .catch((error) => {
                console.info('Authorization failed:', error.message);
                router.push('/login');
            });
    }, [invoices.loading, invoices.loaded]);

    return (
        <div className="min-h-screen bg-base-200">
            <AppNavbar />
            {/* Render Pie chart only if calculation data is available */}
            {calculation && calculation.scopes && (
                <div className="p-6">
                    <Pie className={'max-w-[256px] max-h-[256px]'}
                        data={chartData} />
                </div>
            )}
        </div>
    );
}
