import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  // Encode the search params (the ?... part) so that ampersands (&)
  // are treated as text, not as separators for the login page parameters.
  const pathAndQuery = `${request.nextUrl.pathname}${encodeURIComponent(request.nextUrl.search)}`;

  const shouldHandle = !request.nextUrl.pathname.includes('/api/');

  if (shouldHandle) {
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
}

export const config = {
  matcher: [
    // All routes except /login, /api, /_next, and /favicon.ico
    '/((?!api|_next/static|_next/image|favicon.ico).*)',
  ],
};
