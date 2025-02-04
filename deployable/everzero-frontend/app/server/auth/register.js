'use server';

import axios from 'axios';
import { cookies } from 'next/headers'; // For managing cookies in Next.js

export async function register(dto) {
    const API_URL = process.env.API_URL;

    try {
        // Make the API request
        const response = await axios.post(`${API_URL}/api/register`, dto, {
            headers: {
                "Content-Type": "application/json",
            },
        });

        const { token } = response.data; // Assuming the token is returned as `token`

        // Set the token as a secure, HTTP-only cookie with expiration matching the JWT
        cookies().set({
            name: 'auth_token',
            value: token,
            httpOnly: true, // Prevent JavaScript access
            secure: process.env.NODE_ENV === 'production', // Use HTTPS in production
            sameSite: 'strict', // Prevent CSRF
            path: '/', // Available across the domain
            maxAge: 60 * 60 // Set cookie lifetime to 1 hour
        });

        return {
            success: true,
            data: response.data,
        };
    } catch (err) {
        // Log error in development mode
        if (process.env.NODE_ENV !== 'production') {
            console.error('Registration error:', err.response?.data || err.message);
        }

        // Return a secure error message
        return {
            success: false,
            message: "An error occurred during registration. Please try again later.",
        };
    }
}
