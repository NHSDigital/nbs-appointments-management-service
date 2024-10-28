import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const pathAndQuery = `${request.nextUrl.pathname}${request.nextUrl.search}`;

  return NextResponse.next({
    headers: {
      'mya-last-requested-path': pathAndQuery,
    },
  });
}

export const config = {
  matcher: [
    // All routes except /login, /api, /_next, and /favicon.ico
    '/((?!api|_next/static|_next/image|favicon.ico|login).*)',
  ],
};
