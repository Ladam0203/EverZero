"use client"; // If using Next.js App Router with React Server Components

import Link from "next/link";
import { usePathname } from "next/navigation"; // For Next.js App Router
import { FaFileInvoiceDollar, FaFileAlt, FaChartBar } from "react-icons/fa";

export default function AppNavbar() {
    const pathname = usePathname(); // Get the current path

    return (
        <div className="w-full bg-base-100"> {/* Full-width background */}
            <div className="container mx-auto"> {/* Centered content */}
                <div className="navbar"> {/* Navbar container */}
                    <div className="navbar-start gap-6">
                        <Link
                            href="/dashboard"
                            className={`btn btn-ghost ${
                                pathname === "/dashboard" ? "btn-active btn-outline" : ""
                            }`}
                        >
                            <FaChartBar className="mr-2" />
                            Dashboard
                        </Link>
                        <Link
                            href="/invoices"
                            className={`btn btn-ghost ${
                                pathname === "/invoices" ? "btn-active btn-outline" : ""
                            }`}
                        >
                            <FaFileInvoiceDollar className="mr-2" />
                            Invoices
                        </Link>
                        <Link
                            href="/reports"
                            className={`btn btn-ghost ${
                                pathname === "/reports" ? "btn-active btn-outline" : ""
                            }`}
                        >
                            <FaFileAlt className="mr-2" />
                            Reports
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}