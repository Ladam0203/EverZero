import axios from 'axios';
import { cookies } from 'next/headers';
import { NextResponse } from 'next/server';

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function POST(request) {
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

        // Parse the request body
        const body = await request.json();

        // Parse dates to PostInvoice DTO
        body.forEach((invoice) => {
            invoice.date = new Date(invoice.date).toISOString();

            // Remove emissionFactorId if not provided
            if (!invoice.emissionFactorId) {
                delete invoice.emissionFactorId;
            }
        });

        // Make the API call with axios
        const response = await axios.post(`${API_URL}/invoices/bulk`, body, {
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
        console.error('Error response:', error.response?.data || error.message);

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
                    message: 'Could not upload invoices. Please try again later.',
                },
                { status: 502 }
            );
        }

        // Handle body parsing errors
        if (error instanceof SyntaxError && error.message.includes('JSON')) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'Invalid JSON in request body.',
                },
                { status: 400 }
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