'use server';
import { cookies } from 'next/headers';
import { NextRequest } from 'next/server';

export async function GET(request: NextRequest) {
  const startDate = request.nextUrl.searchParams.get('startDate');
  const endDate = request.nextUrl.searchParams.get('endDate');

  const cookieStore = await cookies();

  const tokenCookie = cookieStore.get('token');

  return await fetch(
    `${process.env.NBS_API_BASE_URL}/api/report/site-summary?startDate=${startDate}&endDate=${endDate}`,
    {
      method: 'GET',
      headers: tokenCookie
        ? {
            Authorization: `Bearer ${tokenCookie.value}`,
          }
        : undefined,
    },
  ).then(fileResponse => {
    return fileResponse.blob().then(blob => {
      return new Response(blob, {
        headers: fileResponse.headers,
      });
    });
  });
}
