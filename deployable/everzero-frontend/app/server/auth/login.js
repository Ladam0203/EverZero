'use server';

import axios from 'axios';
import { cookies } from 'next/headers'; // For managing cookies in Next.js

export async function login(dto) {
    const API_URL = process.env.NEXT_PUBLIC_API_URL;

    try {
        const response = await axios.post(`${API_URL}/login`, dto, {
            headers: {
                "Content-Type": "application/json",
            },
        });

        const { token } = response.data; // Assuming the token is returned as `token`

        // Check if token exists and is valid
        if (!token) {
            return {
                success: false,
                message: "Authentication failed, token missing",
            };
        }

        // Set the token as a secure, HTTP-only cookie with expiration matching the JWT
        cookies().set({
            name: 'auth_token',
            value: token,
            httpOnly: true, // Prevent JavaScript access
            secure: process.env.NODE_ENV === 'production', // Use HTTPS in production
            sameSite: 'strict', // Prevent CSRF
            path: '/', // Available across the domain
            maxAge: 60 * 60, // Set cookie lifetime to 1 hour
        });

        return {
            success: true,
            data: response.data,
        };
    } catch (err) {
        // Log the detailed error on the server, return generic error to client
        console.error('Login error:', err.response?.data || err.message);
        return {
            success: false,
            message: "Invalid login credentials", // Avoid sending sensitive error details
        };
    }
}
