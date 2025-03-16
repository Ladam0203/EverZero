'use client'

import AppNavbar from "@/app/components/AppNavbar";
import { useRouter } from "next/navigation";
import { useAtom } from "jotai";
import { invoicesAtom } from "@/app/atoms/invoicesAtom";
import { authorize } from "@/app/server/auth/authorize";
import { useEffect, useState } from "react";
import { Doughnut, Bar } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend,
    CategoryScale,
    LinearScale,
    BarElement,
    Title
} from 'chart.js';
import { FaSpinner } from "react-icons/fa";

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
                backgroundColor: ['#4768fa', '#7b92b2', '#67cba0'],
                borderWidth: 0,
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
                    '#1c92f2', '#009485', '#ff9900', '#ff5724', '#4768fa', '#7b92b2', '#67cba0',
                ],
                borderWidth: 0,
                hoverOffset: 4,
            },
        ],
    });

    // New function to generate monthly stacked chart data from backend Years data
    const generateMonthlyChartData = (calculation) => {
        const currentYear = calculation?.years?.find(y => y.year === new Date().getFullYear().toString());
        if (!currentYear) return { labels: [], datasets: [] };

        const months = currentYear.months || [];
        const allCategories = [...new Set(months.flatMap(m => m.categories.map(c => c.category)))];

        const datasets = allCategories.map((category, index) => ({
            label: category,
            data: months.map(month => {
                const categoryData = month.categories.find(c => c.category === category);
                return categoryData?.emission || 0;
            }),
            backgroundColor: [
                '#1c92f2', '#009485', '#ff9900', '#ff5724', '#4768fa', '#7b92b2', '#67cba0',
            ][index % 7],
            borderWidth: 0,
        }));

        return {
            labels: months.map(m => m.month),
            datasets: datasets,
        };
    };

    useEffect(() => {
        const doAuthorize = async () => {
            const response = await authorize();
            if (!response.authenticated) throw new Error('Unauthorized');
        };

        const fetchInvoices = async () => {
            if (invoices.loading || invoices.loaded) return;

            setInvoices((prev) => ({ ...prev, loading: true }));

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

            result.data.scopes.sort((a, b) => a.scope.localeCompare(b.scope));
            setCalculation(result.data);
            console.log(result.data);
        };

        doAuthorize()
            .then(() => fetchInvoices())
            .then(() => fetchCalculation())
            .catch((error) => {
                console.info('Authorization failed:', error.message);
                router.push('/login');
            });
    }, [invoices.loading, invoices.loaded, router, setInvoices]);

    const chartOptions = {
        maintainAspectRatio: true,
        plugins: {
            legend: {
                position: "bottom",
                labels: {
                    generateLabels: (chart) => {
                        const { data } = chart;
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
                                    lineDash: [5, 5],
                                };
                            });
                        }
                        return [];
                    },
                    padding: 20,
                    usePointStyle: true,
                },
            },
            tooltip: {
                callbacks: {
                    label: (context) => `${context.label}: ${context.parsed} kg`,
                },
            },
        },
    };

    const barChartOptions = {
        maintainAspectRatio: true,
        scales: {
            x: { stacked: true, title: { display: true } },
            y: {
                stacked: true,
                title: { display: true, text: 'Emissions (kg)' },
                beginAtZero: true,
            },
        },
        plugins: {
            legend: {
                position: "bottom",
                labels: { padding: 20, usePointStyle: true },
            },
            tooltip: {
                callbacks: {
                    label: (context) => `${context.dataset.label}: ${context.parsed.y} kg`,
                },
            },
        },
    };

    return (
        <div className="min-h-screen bg-gray-100">
            <AppNavbar />
            <div className="container mx-auto px-4 py-8">
                <h1 className="text-3xl font-bold mb-8 text-gray-800">
                    Dashboard
                    <span className="text-3xl font-normal text-gray-500 ml-2">({new Date().getFullYear()})</span>
                </h1>

                {!calculation ? (
                    <div className="flex justify-center items-center h-64">
                        <FaSpinner className="animate-spin text-4xl text-blue-600" />
                    </div>
                ) : (
                    <div className="space-y-8">
                        {/* Summary Cards */}
                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                            {[
                                { title: `Total Emission`, value: `${calculation.totalEmission.toFixed(1)} kg CO2e` },
                                {
                                    title: `Avg Monthly Emission`,
                                    value: `${
                                        calculation.years.find(y => y.year === new Date().getFullYear().toString())
                                            ?.averageMonthlyEmission.toFixed(1) || 0
                                    } kg CO2e`,
                                },
                                {
                                    title: `Highest Scope`,
                                    value: calculation.scopes.reduce((a, b) => a.emission > b.emission ? a : b).scope,
                                },
                                {
                                    title: `Highest Activity`,
                                    value: calculation.scopes
                                        .reduce((a, b) => a.emission > b.emission ? a : b)
                                        .categories.reduce((a, b) => a.emission > b.emission ? a : b).category,
                                },
                            ].map((item, index) => (
                                <div key={index} className="bg-white p-6 rounded-xl shadow-sm hover:shadow-md transition-shadow">
                                    <h3 className="text-sm font-medium text-gray-600">{item.title}</h3>
                                    <p className="text-2xl font-bold text-gray-800 mt-2">{item.value}</p>
                                </div>
                            ))}
                        </div>

                        {/* Charts Section */}
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                            {/* Monthly Carbon Footprint */}
                            <div className="bg-white p-6 rounded-xl shadow-sm flex flex-col items-center">
                                <h2 className="text-lg font-semibold text-gray-800 mb-4">Carbon Footprint</h2>
                                <div className="w-full h-[32rem] mt-auto">
                                    <Bar data={generateMonthlyChartData(calculation)} options={barChartOptions} />
                                </div>
                            </div>

                            {/* Scope Charts */}
                            <div className="grid grid-cols-2 gap-6">
                                <div className="bg-white p-6 rounded-xl shadow-sm flex flex-col items-center">
                                    <h2 className="text-lg font-semibold text-gray-800 mb-4">Scope Breakdown</h2>
                                    <div className="w-full h-64 mx-auto">
                                        <Doughnut className="mx-auto" data={overallChartData} options={chartOptions} />
                                    </div>
                                </div>

                                {calculation.scopes.map((scope, index) => (
                                    <div key={index} className="bg-white p-6 rounded-xl shadow-sm flex flex-col items-center">
                                        <h3 className="text-lg font-semibold text-gray-800 mb-4">{scope.scope} Activities</h3>
                                        <div className="w-full h-64 mx-auto">
                                            <Doughnut className="mx-auto" data={generateCategoryChartData(scope)} options={chartOptions} />
                                        </div>
                                    </div>
                                ))}
                                {Array.from({ length: Math.max(0, 3 - calculation.scopes.length) }).map((_, index) => (
                                    <div key={`empty-${index}`} className="bg-white p-6 rounded-xl shadow-sm invisible"></div>
                                ))}
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}