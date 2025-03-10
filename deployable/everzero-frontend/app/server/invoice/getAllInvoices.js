'use server';

import axios from 'axios';
import axiosRetry from 'axios-retry';
import { cookies } from 'next/headers';

const API_URL = process.env.NEXT_PUBLIC_API_URL;

axiosRetry(axios, {
    retries: 3,
    retryDelay: (retryCount) => 1000 * Math.pow(2, retryCount), // Exponential backoff
    retryCondition: (error) => {
        // Retry only for network or 5xx errors
        return error.response?.status === 502 || !error.response;
    },
});

export async function getAllInvoices() {
    const token = (await cookies()).get('auth_token');

    if (!token) {
        return {
            success: false,
            message: "Authentication token is missing or expired.",
        };
    }

    try {
        const response = await axios.get(`${API_URL}/invoices`, {
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token.value}`,
            },
        });

        return {
            success: true,
            data: response.data,
        };
    } catch (err) {
        if (err.response?.status === 401) {
            return {
                success: false,
                message: "Authentication token is missing or expired.",
            };
        }

        if (err.response?.status === 502) {
            return {
                success: false,
                message: "Could not load invoices. Please try again later.",
            };
        }

        return {
            success: false,
            message: err.response?.data || err.message || "An unknown error occurred.",
        };
    }
}
