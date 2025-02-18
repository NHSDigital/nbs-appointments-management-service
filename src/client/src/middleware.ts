import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const pathAndQuery = `${request.nextUrl.pathname}${request.nextUrl.search}`;

  const nextArguments = request.nextUrl.pathname.endsWith('login')
    ? {}
    : {
        headers: {
          'mya-last-requested-path': pathAndQuery,
        },
      };

  const response = NextResponse.next(nextArguments);

  response.headers.set(
    'x-forwarded-host',
    request.headers.get('origin')?.replace(/(http|https):\/\//, '') || '*',
  );

  return response;
}

export const config = {
  matcher: [
    // All routes except /login, /api, /_next, and /favicon.ico
    '/manage-your-appointments/((?!api|_next/static|_next/image|favicon.ico).*)',
  ],
};
