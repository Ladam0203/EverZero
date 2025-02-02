"use client"

import { FaLeaf, FaChartLine, FaUsers, FaCog } from "react-icons/fa"
import Navbar from "./components/navbar"

export default function Home() {
    return (
        <main className="min-h-screen">
            <Navbar />

            {/* Hero Section */}
            <section className="hero min-h-screen bg-base-200">
                <div className="hero-content text-center">
                    <div className="max-w-md">
                        <h1 className="text-5xl font-bold">Carbon Emission Reports, Made Simple</h1>
                        <p className="py-6">
                            Simple, affordable, and user-friendly carbon emission reporting software.
                        </p>
                        <div className="flex justify-center space-x-4">
                            <button className="btn btn-primary">Get Started</button>
                            <button className="btn btn-outline">Learn More</button>
                        </div>
                    </div>
                </div>
            </section>

            {/* Features Section */}
            <section className="py-16">
                <div className="container mx-auto px-4">
                    <h2 className="text-3xl font-bold text-center mb-12">Our Features</h2>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                        <FeatureCard
                            icon={<FaLeaf className="text-4xl text-primary" />}
                            title="Easy Reporting"
                            description="Generate comprehensive carbon emission reports with just a few clicks."
                        />
                        <FeatureCard
                            icon={<FaChartLine className="text-4xl text-primary" />}
                            title="Intuitive Interface"
                            description="User-friendly design requires no extensive training or technical expertise."
                        />
                        <FeatureCard
                            icon={<FaUsers className="text-4xl text-primary" />}
                            title="Affordable"
                            description="Pricing tailored for small companies and non-profit organizations."
                        />
                        <FeatureCard
                            icon={<FaCog className="text-4xl text-primary" />}
                            title="Automated Processing"
                            description="Streamlined data collection and processing to save time and effort."
                        />
                    </div>
                </div>
            </section>

            {/* Call to Action Section */}
            <section className="bg-base-200 py-16">
                <div className="container mx-auto px-4 text-center">
                    <h2 className="text-3xl font-bold mb-6">Ready to Start Reporting?</h2>
                    <p className="mb-8">Join the growing number of organizations making a difference.</p>
                    <button className="btn btn-primary btn-lg">Sign Up Now</button>
                </div>
            </section>

            {/* Footer */}
            <footer className="footer footer-center p-10 bg-base-300 text-base-content">
                <div>
                    <p>© 2025 EverZero. All rights reserved.</p>
                </div>
            </footer>
        </main>
    )
}

function FeatureCard({ icon, title, description }) {
    return (
        <div className="card bg-base-100 shadow-xl">
            <div className="card-body items-center text-center">
                {icon}
                <h3 className="card-title">{title}</h3>
                <p>{description}</p>
            </div>
        </div>
    )
}

