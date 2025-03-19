import axios from 'axios';
import { cookies } from 'next/headers';
import { NextResponse } from 'next/server';

export async function POST(request) {
    const API_URL = process.env.NEXT_PUBLIC_API_URL;

    const formData = await request.formData();
    const file = formData.get('files');
    const token = (await cookies()).get('auth_token')?.value;

    if (!file) {
        return NextResponse.json({ success: false, message: 'No file provided' }, { status: 400 });
    }
    if (!token) {
        return NextResponse.json({ success: false, message: 'Authentication required' }, { status: 401 });
    }

    try {
        const response = await axios.post(`${API_URL}/extract/bulk`, formData, {
            headers: {
                'Authorization': `Bearer ${token}`,
            },
        });
        return NextResponse.json({ success: true, data: response.data }, { status: 200 });
    } catch (error) {
        return NextResponse.json(
            { success: false, message: error.message || 'An error occurred' },
            { status: error.response?.status || 500 }
        );
    }
}