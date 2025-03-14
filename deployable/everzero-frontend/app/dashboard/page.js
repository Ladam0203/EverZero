'use client'

import AppNavbar from "@/app/components/AppNavbar";
import {useRouter} from "next/navigation";
import {useAtom} from "jotai";
import {invoicesAtom} from "@/app/atoms/invoicesAtom";
import {authorize} from "@/app/server/auth/authorize";
import {useEffect, useState} from "react";
import {Doughnut, Bar} from 'react-chartjs-2';
import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend,
    CategoryScale,
    LinearScale,
    BarElement,
    Title // Added Title for axis labels
} from 'chart.js';
import {FaPlus, FaSpinner} from "react-icons/fa";

// Register all necessary components
ChartJS.register(
    ArcElement,
    Tooltip,
    Legend,
    CategoryScale,
    LinearScale,
    BarElement,
    Title
);

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
                    '#4768fa',
                    '#7b92b2',
                    '#67cba0',
                ],
                borderWidth: 0,
                hoverOffset: 4,
            },
        ],
    }

    // Function to generate chart data for each scope's categories
    const generateCategoryChartData = (scope) => ({
        labels: scope.categories?.map((category) => category.category) || [],
        datasets: [
            {
                data: scope.categories?.map((category) => category.emission) || [],
                backgroundColor: [
                    '#1c92f2',
                    '#009485',
                    '#ff9900',
                    '#ff5724',
                    '#4768fa',
                    '#7b92b2',
                    '#67cba0',
                ],
                borderWidth: 0,
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

            // Sort by scope
            result.data.scopes.sort((a, b) => a.scope.localeCompare(b.scope));

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

    const getMonthlyEmissions = (calculation) => {
        // Get what months are there in the invoices
        const months = calculation.invoices.map((invoice) => invoice.date.split('-')[1]);
        const uniqueMonths = [...new Set(months)];

        // Calculate emissions for each month
        const monthlyEmissions = uniqueMonths.map((month) => {
            const monthInvoices = calculation.invoices.filter((invoice) => invoice.date.split('-')[1] === month);
            const monthEmission = monthInvoices.reduce((acc, invoice) => acc + invoice.emission, 0);
            return monthEmission;
        });

        return monthlyEmissions;
    }

    const chartOptions = {
        maintainAspectRatio: true,
        plugins: {
            legend: {
                position: "bottom", // Move legend to the right (or 'left', 'top', etc.)
                labels: {
                    generateLabels: (chart) => {
                        const {data} = chart;
                        if (data.labels.length && data.datasets.length) {
                            return data.labels.map((label, i) => {
                                const meta = chart.getDatasetMeta(0);
                                const style = meta.controller.getStyle(i);
                                return {
                                    text: label,
                                    fillStyle: style.backgroundColor,
                                    strokeStyle: style.borderColor,
                                    lineWidth: style.borderWidth,
                                    hidden: !chart.getDataVisibility(i),
                                    index: i,
                                    // Add a leader line effect
                                    lineDash: [5, 5], // Optional: dashed line for visual effect
                                };
                            });
                        }
                        return [];
                    },
                    padding: 20, // Space between legend items
                    usePointStyle: true, // Use a point style instead of a box
                },
            },
            tooltip: {
                callbacks: {
                    label: (context) => {
                        const label = context.label || "";
                        const value = context.parsed || 0;
                        return `${label}: ${value}`;
                    },
                },
            },
        },
    };

    const generateMonthlyStackedChartData = (calculation) => {
        const months = calculation.invoices.map((invoice) => invoice.date.split('-')[1]);
        const uniqueMonths = [...new Set(months)].sort();

        // Get all unique categories across all scopes
        const allCategories = [...new Set(
            calculation.scopes.flatMap(scope =>
                scope.categories.map(cat => cat.category)
            )
        )];

        // Calculate emissions per category per month
        const datasets = allCategories.map((category, index) => ({
            label: category,
            data: uniqueMonths.map(month => {
                const monthInvoices = calculation.invoices.filter(
                    invoice => invoice.date.split('-')[1] === month
                );
                return monthInvoices.reduce((sum, invoice) => {
                    const categoryEmission = calculation.scopes
                        .flatMap(scope => scope.categories)
                        .find(cat => cat.category === category)?.emission || 0;
                    return sum + (categoryEmission / calculation.invoices.length);
                }, 0);
            }),
            backgroundColor: [
                '#1c92f2',
                '#009485',
                '#ff9900',
                '#ff5724',
                '#4768fa',
                '#7b92b2',
                '#67cba0',
            ][index % 7],
            borderWidth: 0,
        }));

        return {
            // Map month number to month name
            labels: uniqueMonths.map(month => {
                return new Date(2021, month - 1).toLocaleString('default', {month: 'long'});
            }),
            datasets: datasets,
        };
    };

    const barChartOptions = {
        maintainAspectRatio: true,
        scales: {
            x: {
                stacked: true,
                title: {
                    display: true,
                }
            },
            y: {
                stacked: true,
                title: {
                    display: true,
                    text: 'Emissions (kg)'
                },
                beginAtZero: true,
            }
        },
        plugins: {
            legend: {
                position: "bottom",
                labels: {
                    padding: 20,
                    usePointStyle: true,
                },
            },
            tooltip: {
                callbacks: {
                    label: (context) => {
                        const label = context.dataset.label || "";
                        const value = context.parsed.y || 0;
                        return `${label}: ${value} kg`;
                    },
                },
            },
        },
    };

    return (
        <div className="min-h-screen bg-base-200">
            <AppNavbar/>

            <section className="p-8">

                <div className="flex justify-between items-center mb-8">
                    <h1 className="text-4xl font-bold">Dashboard</h1>
                </div>

                {!calculation && (
                    <div className="flex justify-center items-center">
                        <FaSpinner className="animate-spin text-4xl text-primary"/>
                    </div>
                )}
                {calculation && calculation.scopes && (
                    <div className={"flex flex-col gap-6"}>
                        <div>
                            {/* Emission Calculation Summary */}
                            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
                                <div className="bg-base-100 p-4 rounded-lg shadow-md">
                                    <h3 className="text-lg font-semibold mb-2">Total Emission</h3>
                                    <p className="text-3xl font-bold">{calculation?.totalEmission} kg</p>
                                </div>
                                <div className="bg-base-100 p-4 rounded-lg shadow-md">
                                    <h3 className="text-lg font-semibold mb-2">Average Monthly Emission</h3>
                                    <p className="text-3xl font-bold">{getMonthlyEmissions(calculation).reduce((a, b) => a + b, 0) / getMonthlyEmissions(calculation).length} kg</p>
                                </div>
                                <div className="bg-base-100 p-4 rounded-lg shadow-md">
                                    <h3 className="text-lg font-semibold mb-2">Highest Scope</h3>
                                    <p className="text-3xl font-bold">{calculation?.scopes?.reduce((a, b) => a.emission > b.emission ? a : b).scope}</p>
                                </div>
                                <div className="bg-base-100 p-4 rounded-lg shadow-md">
                                    <h3 className="text-lg font-semibold mb-2">Highest Activity</h3>
                                    <p className="text-3xl font-bold">{calculation?.scopes?.reduce((a, b) => a.emission > b.emission ? a : b).categories.reduce((a, b) => a.emission > b.emission ? a : b).category}</p>
                                </div>
                            </div>

                            <div className="flex flex-wrap">
                                <div className={"flex flex-col w-full md:w-1/3 pr-3"}>
                                    <div className="bg-white p-4 rounded-lg shadow-md mb-6">
                                        <h2 className="text-lg font-semibold mb-4">Scopes</h2>
                                        <div className="flex justify-center max-w-1/3 max-h-[256px]">
                                            <Doughnut
                                                data={overallChartData}
                                                options={chartOptions}
                                            />
                                        </div>
                                    </div>

                                    {/* Individual Scope Category Charts */}
                                    <div className="flex gap-6">
                                        {calculation.scopes.map((scope, index) => (
                                            <div key={index} className="bg-white rounded-lg shadow-md w-1/3">
                                                <h2 className="text-lg font-semibold mb-4 p-4">{scope.scope}</h2>
                                                <div className="flex justify-center">
                                                    <Doughnut
                                                        data={generateCategoryChartData(scope)}
                                                        options={chartOptions}
                                                    />
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                                <div className={"flex flex-col gap-6 w-full md:w-2/3 pl-3"}>
                                    <div className="bg-white p-4 rounded-lg shadow-md">
                                        <h2 className="text-lg font-semibold mb-4">Carbon Footprint</h2>
                                        <div>
                                            <Bar
                                                data={generateMonthlyStackedChartData(calculation)}
                                                options={barChartOptions}
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </section>
        </div>
    );
}
