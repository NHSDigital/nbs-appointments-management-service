'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';

export async function GET(request: NextRequest) {
  const startDate = request.nextUrl.searchParams.get('startDate');
  const endDate = request.nextUrl.searchParams.get('endDate');

  const apiUrl = `${process.env.NBS_API_BASE_URL}/api/report/site-summary?startDate=${startDate}&endDate=${endDate}`;

  redirect(apiUrl);
}
