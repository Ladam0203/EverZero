import Link from "next/link"

export default function LandingNavbar() {

    return (
        <div className="navbar bg-base-100">
            <div className="navbar-start">
                <Link href="/" className="btn btn-ghost normal-case text-xl">
                    EverZero
                </Link>
            </div>
            <div className="navbar-end">
                <Link href="/login" className="btn btn-ghost">
                    Login
                </Link>
                <Link href="/register" className="btn btn-primary">
                    Register
                </Link>
            </div>
        </div>
    )
}

