"use client";

import { useState } from "react";
import { FaEye, FaEyeSlash } from "react-icons/fa";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useSetAtom } from "jotai";
import { authAtom } from "@/app/atoms/authAtom";
import { register } from "@/app/server/auth/register";
import process from "next/dist/build/webpack/loaders/resolve-url-loader/lib/postcss";

export default function Register() {
    const router = useRouter();
    const setAuthState = useSetAtom(authAtom);
    const [showPassword, setShowPassword] = useState(false);
    const [email, setEmail] = useState( "");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState("");

    const validateUsername = (username) => {
        const allowedCharacters = /^[a-zA-Z0-9\-._@+]+$/;
        return allowedCharacters.test(username);
    };

    const validatePassword = (password) => {
        const minLength = 6;
        const requireDigit = /\d/;
        const requireLowercase = /[a-z]/;
        const requireUppercase = /[A-Z]/;
        const requireNonAlphanumeric = /[\W_]/;
        return (
            password.length >= minLength &&
            requireDigit.test(password) &&
            requireLowercase.test(password) &&
            requireUppercase.test(password) &&
            requireNonAlphanumeric.test(password)
        );
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");

        if (!validateUsername(username)) {
            setError("Username can only contain letters, numbers, and -._@+");
            return;
        }

        if (!validatePassword(password)) {
            setError(
                "Password must be at least 6 characters long, contain at least one digit, one lowercase, one uppercase, and one special character."
            );
            return;
        }

        if (password !== confirmPassword) {
            setError("Passwords do not match!");
            return;
        }

        const dto = { email, username, password };
        const result = await register(dto);

        if (!result.success) {
            setError(result.message);
            return;
        }

        const data = result.data;
        setAuthState({
            isAuthenticated: true,
            user: {
                id: data.id,
                email: data.email,
                username: data.username,
                //roles: data.roles,
            },
            token: data.token,
        });

        console.log("Registered user:", data);
        console.info("User registered successfully!");

        router.push("/dashboard");
    };

    return (
        <div className="min-h-screen bg-base-200 flex items-center justify-center">
            <div className="card w-full max-w-md bg-base-100 shadow-xl p-6">
                <h2 className="card-title text-3xl font-bold text-center mb-6">Register</h2>
                {error && <div className="text-red-600 text-sm text-center mb-4">{error}</div>}
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
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
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
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
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
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
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
                                value={confirmPassword}
                                onChange={(e) => setConfirmPassword(e.target.value)}
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
                        Already have an account?{' '}
                        <Link href="/login" className="link link-primary">
                            Login here
                        </Link>
                    </p>
                </div>
            </div>
        </div>
    );
}
