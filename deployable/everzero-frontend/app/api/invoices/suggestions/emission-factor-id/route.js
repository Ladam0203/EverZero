import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import axios from "axios";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function GET(request) {
    try {
        const { searchParams } = new URL(request.url);
        const supplierName = searchParams.get('supplierName');
        const invoiceLineDescription = searchParams.get('invoiceLineDescription');
        const unit = searchParams.get('unit');

        if (!supplierName || !invoiceLineDescription || !unit) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'Missing required query parameters.',
                },
                { status: 400 }
            );
        }

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

        const response = await axios.get(`${API_URL}/invoices/suggestions/emission-factor-id`, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token.value}`,
            },
            params: {
                supplierName,
                invoiceLineDescription,
                unit,
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
        if (error.response?.status === 401) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'Authentication token is missing or expired.',
                },
                { status: 401 }
            );
        }

        if (error.response?.status === 404) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'No emission factor suggestion found.',
                },
                { status: 404 }
            );
        }

        if (error.response?.status === 502) {
            return NextResponse.json(
                {
                    success: false,
                    message: 'Could not load suggestions. Please try again later.',
                },
                { status: 502 }
            );
        }

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
