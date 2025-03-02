import axios from 'axios';
import { cookies } from 'next/headers';
import { NextResponse } from 'next/server';

// POST handler for the API route
export async function POST(request) {
    const API_URL = process.env.NEXT_PUBLIC_API_URL;

    try {
        // Get request body
        const dto = await request.json();

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

        // Make the API call
        const response = await axios.post(`${API_URL}/calculate`,
            dto, {
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