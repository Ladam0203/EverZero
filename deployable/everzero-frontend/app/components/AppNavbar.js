"use client"; // If using Next.js App Router with React Server Components

import Link from "next/link";
import { usePathname } from "next/navigation"; // For Next.js App Router

export default function AppNavbar() {
    const pathname = usePathname(); // Get the current path

    return (
        <div className="navbar bg-base-100">
            <div className="navbar-start">
                <Link href="/invoices" className={`btn btn-ghost ${pathname === "/invoices" ? "btn-active btn-outline" : ""}`}>
                    Invoices
                </Link>
                <Link href="/reports" className={`btn btn-ghost ${pathname === "/reports" ? "btn-active btn-outline" : ""}`}>
                    Reports
                </Link>
            </div>
        </div>
    );
}
