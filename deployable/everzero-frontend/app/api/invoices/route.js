// app/api/invoices/route.js
import axios from 'axios';
import axiosRetry from 'axios-retry';
import { cookies } from 'next/headers';
import { NextResponse } from 'next/server';

const API_URL = process.env.NEXT_PUBLIC_API_URL;

// Configure axios-retry
axiosRetry(axios, {
    retries: 3,
    retryDelay: (retryCount) => 1000 * Math.pow(2, retryCount), // Exponential backoff
    retryCondition: (error) => {
        // Retry only for network or 5xx errors
        return error.response?.status === 502 || !error.response;
    },
});

// GET handler for the API route
export async function GET(request) {
    try {
        // Retrieve the JWT token from cookies
        const cookieStore = await cookies();
        const token = cookieStore.get('auth_token');

        if (!token) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'Authentication token is missing or expired.',
                },
                { status: 401 }
            );
        }

        const startDate = request.nextUrl.searchParams.get('startDate');
        const endDate = request.nextUrl.searchParams.get('endDate');

        let url = `${API_URL}/invoices`;
        if (startDate) {
            url += `?startDate=${startDate}`;
        }
        if (endDate) {
            url += `${startDate ? '&' : '?'}endDate=${endDate}`;
        }

        // Make the API call with axios
        const response = await axios.get(url, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token.value}`,
            },
        });

        return NextResponse.json(
            {
                success: true,
                data: response.data,
            },
            { status: 200 }
        );
    } catch (error) {
        console.error(error);

        // Handle specific error cases
        if (error.response?.status === 401) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'Authentication token is missing or expired.',
                },
                { status: 401 }
            );
        }

        if (error.response?.status === 502) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'Could not load invoices. Please try again later.',
                },
                { status: 502 }
            );
        }

        // Generic error handling
        return NextResponse.json(
            {
                success: false,
                message:
                    error.response?.data?.message ||
                    error.message ||
                    'An unknown error occurred.',
            },
            { status: error.response?.status || 500 }
        );
    }
}