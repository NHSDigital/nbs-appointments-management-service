'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';

export async function GET(request: NextRequest) {
  const site = request.nextUrl.searchParams.get('site');
  const startDate = request.nextUrl.searchParams.get('startDate');
  const endDate = request.nextUrl.searchParams.get('endDate');

  // TODO: Set this to the correct value once APPT-1042 is done
  const apiUrl = `${process.env.NBS_API_BASE_URL}/api/reports/download?site=${site}&startDate=${startDate}&endDate=${endDate}`;

  redirect(apiUrl);
}
