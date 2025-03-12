import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import axios from "axios";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function DELETE(request, { params }) {
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

        const { invoiceId } = await params;

        // Make the API call with axios
        await axios.delete(`${API_URL}/invoices/${invoiceId}`, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token.value}`,
            },
        });

        return NextResponse.json({ success: true }, { status: 200 });
    } catch (error) {
        console.error(error);

        if (axios.isAxiosError(error)) {
            const status = error.response?.status;

            if (status === 401) {
                return NextResponse.json({ error: 'Unauthorized access' }, { status: 401 });
            }

            if (status === 404) {
                return NextResponse.json({ error: 'Invoice not found' }, { status: 404 });
            }

            return NextResponse.json(
                { error: error.response?.data?.message || 'Unexpected error occurred' },
                { status: status || 500 }
            );
        }

        return NextResponse.json({ error: 'Internal Server Error' }, { status: 500 });
    }
}
