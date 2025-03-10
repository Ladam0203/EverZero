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
    const [calculation, setCalculation] = useState(null);

    // Chart data for the Overall scope emissions
    const overallChartData = {
        labels: calculation?.scopes?.map((scope) => scope.scope) || [],
        datasets: [
            {
                data: calculation?.scopes?.map((scope) => scope.emission) || [],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                ],
                borderWidth: 1,
                hoverOffset: 4,
            },
        ],
    };

    // Function to generate chart data for each scope's categories
    const generateCategoryChartData = (scope) => ({
        labels: scope.categories?.map((category) => category.category) || [],
        datasets: [
            {
                data: scope.categories?.map((category) => category.emission) || [],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(255, 206, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                ],
                borderWidth: 1,
                hoverOffset: 4,
            },
        ],
    });

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
        };

        const fetchCalculation = async () => {
            if (invoices.loading || !invoices.loaded) return;

            const dto = invoices.invoices;
            const result = await fetch('api/calculate', {
                method: 'POST',
                body: JSON.stringify(dto),
            }).then((res) => res.json());

            if (!result.success) {
                setCalculation(null);
                return;
            }

            setCalculation(result.data);
        };

        doAuthorize()
            .then(() => fetchInvoices())
            .then(() => fetchCalculation())
            .catch((error) => {
                console.info('Authorization failed:', error.message);
                router.push('/login');
            });
    }, [invoices.loading, invoices.loaded, router, setInvoices]);

    return (
        <div className="min-h-screen bg-base-200">
            <AppNavbar />
            {calculation && calculation.scopes && (
                <div className="p-6">
                    {/* Overall Scope Emissions Chart */}
                    <div className="bg-white p-4 rounded-lg shadow-md mb-6">
                        <h2 className="text-lg font-semibold mb-4">Scope Emissions</h2>
                        <div className="max-w-[256px] max-h-[256px] mx-auto">
                            <Pie
                                data={overallChartData}
                                options={{
                                    maintainAspectRatio: true,
                                    plugins: {
                                        legend: {
                                            position: 'bottom',
                                        },
                                        tooltip: {
                                            callbacks: {
                                                label: (context) => {
                                                    const label = context.label || '';
                                                    const value = context.parsed || 0;
                                                    return `${label}: ${value}`;
                                                },
                                            },
                                        },
                                    },
                                }}
                            />
                        </div>
                    </div>

                    {/* Individual Scope Category Charts */}
                    <div className="flex flex-wrap gap-6">
                        {calculation.scopes.map((scope, index) => (
                            <div key={index} className="bg-white p-4 rounded-lg shadow-md">
                                <h2 className="text-lg font-semibold mb-4">{scope.scope} Activity Emissions</h2>
                                <div className="max-w-[256px] max-h-[256px]">
                                    <Pie
                                        data={generateCategoryChartData(scope)}
                                        options={{
                                            maintainAspectRatio: true,
                                            plugins: {
                                                legend: {
                                                    position: 'bottom',
                                                },
                                                tooltip: {
                                                    callbacks: {
                                                        label: (context) => {
                                                            const label = context.label || '';
                                                            const value = context.parsed || 0;
                                                            return `${label}: ${value}`;
                                                        },
                                                    },
                                                },
                                            },
                                        }}
                                    />
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}