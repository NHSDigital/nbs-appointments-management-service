import type { NextRequest } from 'next/server';
import redirectToIdServer from './auth/redirectToIdServer';

export function middleware(request: NextRequest) {
  const authToken = request.cookies.get('token')?.value;

  if (!authToken) {
    redirectToIdServer(request.nextUrl.href);
  }
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|.*\\.png$).*)'],
};
