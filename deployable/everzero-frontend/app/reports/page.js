"use client"
import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAtom } from "jotai"
import { format } from "date-fns"
import { FaFileAlt, FaSpinner, FaPlus, FaExclamationTriangle, FaExternalLinkAlt } from "react-icons/fa"
import { invoicesAtom } from "@/app/atoms/invoicesAtom"
import { reportsAtom } from "@/app/atoms/reportsAtom"
import { authorize } from "@/app/server/auth/authorize"
import { getAllInvoices } from "@/app/server/invoice/getAllInvoices"
import { getAllReports } from "@/app/server/report/getAllReports"
import { calculateEmission } from "@/app/server/emission/calculateEmission"
import { createReport } from "@/app/server/report/createReport"
import AppNavbar from "@/app/components/AppNavbar"

export default function Reports() {
    const router = useRouter()
    const [invoices, setInvoices] = useAtom(invoicesAtom)
    const [reports, setReports] = useAtom(reportsAtom)

    useEffect(() => {
        const doAuthorize = async () => {
            const response = await authorize()
            if (!response.authenticated) {
                throw new Error("Unauthorized")
            }
        }

        const fetchInvoices = async () => {
            if (invoices.loading || invoices.loaded) return

            setInvoices((prev) => ({
                ...prev,
                loading: true,
            }))

            const result = await getAllInvoices()

            if (!result.success) {
                setInvoices((prev) => ({
                    ...prev,
                    loading: false,
                    error: result.message,
                    loaded: true,
                }))
                return
            }

            setInvoices({
                invoices: result.data.length > 0 ? result.data : [],
                loading: false,
                loaded: true,
                error: null,
            })
            console.log("Invoices loaded:", result.data)
        }

        const fetchReports = async () => {
            if (reports.loading || reports.loaded) return

            setReports((prev) => ({
                ...prev,
                loading: true,
            }))

            const result = await getAllReports()

            if (!result.success) {
                setReports((prev) => ({
                    ...prev,
                    loading: false,
                    error: result.message,
                    loaded: true,
                }))
                console.log("Failed to load reports:", result.message)
                return
            }

            setReports({
                reports: result.data.length > 0 ? result.data : [],
                loading: false,
                loaded: true,
                error: null,
            })
            console.log("Reports loaded:", result.data)
        }

        doAuthorize()
            .then(() => fetchInvoices())
            .then(() => fetchReports())
            .catch((error) => {
                console.info("Authorization failed:", error.message)
                router.push("/login")
            })
    }, [invoices.loading, invoices.loaded, reports.loading, reports.loaded, setInvoices, setReports, router])

    const handleCreateReport = async () => {
        const dto = invoices.invoices
        console.log("Creating report with DTO:", dto)

        // Get the emission calculation result
        const calculationResponse = await calculateEmission(dto)
        if (!calculationResponse.success) {
            console.error("Failed to calculate emission:", calculationResponse.message)
            return
        }
        const calculation = calculationResponse.data
        console.log("Emission calculation result:", calculation)

        // Create a PDF report
        const reportResponse = await createReport(calculation)
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
    }

    const handleViewReport = (reportPath) => {
        window.open(`${process.env.NEXT_PUBLIC_API_URL}/${reportPath}`)
    }

    return (
        <div className="min-h-screen bg-base-200">
            <AppNavbar />

            <section className="p-8">
                <div className="flex justify-between items-center mb-8">
                    <h1 className="text-4xl font-bold">Reports</h1>
                    <button className="btn btn-primary" onClick={handleCreateReport}>
                        <FaPlus className="mr-2" /> Create Report
                    </button>
                </div>

                {!reports.loaded ? (
                    <div className="flex justify-center items-center">
                        <FaSpinner className="animate-spin text-4xl text-primary" />
                    </div>
                ) : reports.error ? (
                    <div className="flex justify-center items-center text-red-600">
                        <FaExclamationTriangle className="mr-2" />
                        <span>{reports.error}</span>
                    </div>
                ) : reports.reports.length === 0 ? (
                    <div className="flex justify-center items-center text-yellow-600">
                        <FaExclamationTriangle className="mr-2" />
                        <span>No reports found. Create your first report using the button above.</span>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {reports.reports.map((report) => (
                            <div key={report.id} className="card bg-base-100 shadow-xl">
                                <div className="card-body">
                                    <h2 className="card-title flex items-center">
                                        <FaFileAlt className="text-primary mr-2" />
                                        Emission Report
                                    </h2>
                                    <p>
                                        <strong>Period:</strong> {format(new Date(report.startDate), "MMM d, yyyy")} -{" "}
                                        {format(new Date(report.endDate), "MMM d, yyyy")}
                                    </p>
                                    <p>
                                        <strong>Total Invoices:</strong> {report.totalInvoices}
                                    </p>
                                    <p>
                                        <strong>Total Emission:</strong> {report.totalEmission.toFixed(2)} kg CO₂e
                                    </p>
                                    <div className="card-actions justify-end mt-4">
                                        <button className="btn btn-primary btn-sm" onClick={() => handleViewReport(report.path)}>
                                            View Report <FaExternalLinkAlt className="ml-2" />
                                        </button>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </div>
    )
}

