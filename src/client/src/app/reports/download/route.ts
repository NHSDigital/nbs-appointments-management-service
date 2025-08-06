'use server';
import { fetchSiteSummaryReport } from '@services/appointmentsService';
import { ukNow } from '@services/timeService';
import { NextRequest, NextResponse } from 'next/server';

export async function GET(request: NextRequest) {
  const startDate = request.nextUrl.searchParams.get('startDate');
  const endDate = request.nextUrl.searchParams.get('endDate');
  if (!startDate || !endDate) {
    return new NextResponse('Start date and end date are required', {
      status: 400,
    });
  }

  const fileName = `GeneralSiteSummaryReport-${ukNow().format()}.csv`;

  const blob = await fetchSiteSummaryReport(startDate, endDate);

  return new NextResponse(blob, {
    headers: {
      'Content-Type': 'text/csv',
      'Content-Disposition': `attachment; filename=${fileName}`,
    },
  });
}
