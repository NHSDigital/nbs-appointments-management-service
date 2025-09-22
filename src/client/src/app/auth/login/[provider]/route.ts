import { NextRequest } from 'next/server';
import redirectToIdServer from '../../redirectToIdServer';
import { logEvent } from '../../../lib/logging/logger';

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ provider: string }> },
) {
  logEvent(
    'info',
    'Initiating login redirect',
    {},
    'Auth/Login/[provider]/route.ts',
  );

  const { provider } = await params;
  const redirectUrl = request.nextUrl.searchParams.get('redirectUrl') ?? '/';

  logEvent(
    'info',
    'Redirect URL determined',
    { redirectUrl },
    'Auth/Login/[provider]/route.ts',
  );

  if (!provider) {
    logEvent(
      'error',
      'No provider specified in route parameters',
      {},
      'Auth/Login/[provider]/route.ts',
    );
    throw new Error('No provider specified');
  }
  await redirectToIdServer(redirectUrl, provider);
}
