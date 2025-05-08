import { NextRequest } from 'next/server';
import redirectToIdServer from '../redirectToIdServer';

export async function GET(request: NextRequest) {
  const provider = request.nextUrl.searchParams.get('provider') ?? '';
  const redirectUrl = request.nextUrl.searchParams.get('redirectUrl') ?? '/';

  await redirectToIdServer(redirectUrl, provider);
}
