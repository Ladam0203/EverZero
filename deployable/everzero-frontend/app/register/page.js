"use client"

import { useState } from "react"
import { FaEye, FaEyeSlash } from "react-icons/fa"
import Link from "next/link"

export default function Register() {
    const [showPassword, setShowPassword] = useState(false)

    const handleSubmit = (event) => {
        event.preventDefault()
        // Handle form submission logic here
        console.log("Form submitted")
    }

    return (
        <div className="min-h-screen bg-base-200 flex items-center justify-center">
            <div className="card w-full max-w-md bg-base-100 shadow-xl">
                <div className="card-body">
                    <h2 className="card-title text-3xl font-bold text-center mb-6">Register</h2>
                    <form onSubmit={handleSubmit}>
                        <div className="form-control">
                            <label className="label" htmlFor="username">
                                <span className="label-text">Username</span>
                            </label>
                            <input
                                type="text"
                                id="username"
                                placeholder="Enter your username"
                                className="input input-bordered w-full"
                                required
                            />
                        </div>
                        <div className="form-control mt-4">
                            <label className="label" htmlFor="email">
                                <span className="label-text">Email</span>
                            </label>
                            <input
                                type="email"
                                id="email"
                                placeholder="Enter your email"
                                className="input input-bordered w-full"
                                required
                            />
                        </div>
                        <div className="form-control mt-4">
                            <label className="label" htmlFor="password">
                                <span className="label-text">Password</span>
                            </label>
                            <div className="relative">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    id="password"
                                    placeholder="Enter your password"
                                    className="input input-bordered w-full pr-10"
                                    required
                                />
                                <button
                                    type="button"
                                    className="absolute inset-y-0 right-0 pr-3 flex items-center"
                                    onClick={() => setShowPassword(!showPassword)}
                                >
                                    {showPassword ? <FaEyeSlash className="text-gray-500" /> : <FaEye className="text-gray-500" />}
                                </button>
                            </div>
                        </div>
                        <div className="form-control mt-4">
                            <label className="label" htmlFor="confirmPassword">
                                <span className="label-text">Confirm Password</span>
                            </label>
                            <div className="relative">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    id="confirmPassword"
                                    placeholder="Confirm your password"
                                    className="input input-bordered w-full pr-10"
                                    required
                                />
                                <button
                                    type="button"
                                    className="absolute inset-y-0 right-0 pr-3 flex items-center"
                                    onClick={() => setShowPassword(!showPassword)}
                                >
                                    {showPassword ? <FaEyeSlash className="text-gray-500" /> : <FaEye className="text-gray-500" />}
                                </button>
                            </div>
                        </div>
                        <div className="form-control mt-6">
                            <button type="submit" className="btn btn-primary">
                                Register
                            </button>
                        </div>
                    </form>
                    <div className="text-center mt-4">
                        <p>
                            Already have an account?{" "}
                            <Link href="/login" className="link link-primary">
                                Login here
                            </Link>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    )
}

