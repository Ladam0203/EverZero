'use server';

import axios from 'axios';
import { cookies } from 'next/headers'; // For managing cookies in Next.js

export async function createReport(dto) {
    // TODO: Validate the DTO

    const API_URL = process.env.NEXT_PUBLIC_API_URL;

    // Retrieve the JWT token from the cookies
    const token = (await cookies()).get('auth_token');

    if (!token) {
        return {
            success: false,
            message: "Authentication token is missing or expired.",
        };
    }

    try {
        const response = await axios.post(
            `${API_URL}/reports`, // Endpoint to post a new invoice
            dto, // Corrected: Send dto as the second argument (request body)
            {  // Corrected: Headers should be passed as the third argument
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token.value}`,
                },
            }
        );

        return {
            success: true,
            data: response.data,
        };
    } catch (err) {
        return {
            success: false,
            message: err.response?.data || err.message || "An unknown error occurred.",
        };
    }
}
