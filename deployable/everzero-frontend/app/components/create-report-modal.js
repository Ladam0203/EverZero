import { useState } from 'react';
import { format } from 'date-fns';
import { FaRegCalendarAlt } from 'react-icons/fa';
import {calculateEmission} from "@/app/server/emission/calculateEmission";
import {createReport} from "@/app/server/report/createReport";
import {useAtom} from "jotai/index";
import {reportsAtom} from "@/app/atoms/reportsAtom";

export default function CreateReportModal() {
    const [reports, setReports] = useAtom(reportsAtom)

    const [startDate, setStartDate] = useState(format(new Date(new Date().getFullYear(), 0, 1), 'yyyy-MM-dd'));
    const [endDate, setEndDate] = useState(format(new Date(), 'yyyy-MM-dd'));
    const [includeDetails, setIncludeDetails] = useState(false);

    const handleCreateReport = async (e) => {
        e.preventDefault(); // Prevent the form from reloading the page

        // Fetch the invoices (TODO: In time range)
        const response = await fetch('/api/invoices')
        if (!response.ok) {
            console.error("Failed to fetch invoices:", response.statusText)
            return
        }

        const body = await response.json()
        const invoices = body.data

        // Get the emission calculation result
        const calculationResponse = await calculateEmission(invoices)
        if (!calculationResponse.success) {
            console.error("Failed to calculate emission:", calculationResponse.message)
            return
        }
        const calculation = calculationResponse.data
        console.log("Emission calculation result:", calculation)

        // Create a PDF report
        const reportResponse = await createReport({ emissionCalculation: calculation, shouldIncludePerInvoiceEmissionDetails: includeDetails })
        if (!reportResponse.success) {
            console.error("Failed to generate report:", reportResponse.message)
            return
        }
        const report = reportResponse.data
        console.log("Generated report:", report)

        // Add the new report to the reports list
        setReports((prev) => ({
            ...prev,
            reports: [...prev.reports, report],
        }))

        // Open the report in a new tab
        window.open(`${process.env.NEXT_PUBLIC_API_URL}/${report.path}`)

        // Close the modal
        document.getElementById('create_report_modal').close()
    }

    return (
        <dialog id="create_report_modal" className="modal">
            <div className="modal-box w-fit max-w-5xl">
                <div className="modal-header flex items-center gap-2 mb-4">
                    <FaRegCalendarAlt className="text-primary" />
                    <h3 className="text-lg font-bold">New Report</h3>
                </div>
                <form onSubmit={handleCreateReport} className="space-y-4">
                    <h4 className="text-lg font-bold">Report Period</h4>
                    <div className="flex items-center gap-4">
                        <label className="form-control w-full max-w-xs">
                            <span className="label-text">Start Date</span>
                            <div className="relative">
                                <input
                                    type="date"
                                    value={startDate}
                                    onChange={(e) => setStartDate(e.target.value)}
                                    className="input input-bordered w-full"
                                />
                            </div>
                        </label>
                        <label className="form-control w-full max-w-xs">
                            <span className="label-text">End Date</span>
                            <div className="relative">
                                <input
                                    type="date"
                                    value={endDate}
                                    onChange={(e) => setEndDate(e.target.value)}
                                    className="input input-bordered w-full"
                                />
                            </div>
                        </label>
                    </div>
                    <h4 className="text-lg font-bold">Options</h4>
                    <label className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            checked={includeDetails}
                            onChange={(e) => setIncludeDetails(e.target.checked)}
                            className="checkbox"
                        />
                        <span className="label-text">Include per-invoice emission details</span>
                    </label>
                    <div className="modal-action">
                        <button type="button" className="btn" onClick={() => document.getElementById('create_report_modal').close()}>Cancel</button>
                        <button type="submit" className="btn btn-primary">Generate Report</button>
                    </div>
                </form>
            </div>
        </dialog>
    );
}
