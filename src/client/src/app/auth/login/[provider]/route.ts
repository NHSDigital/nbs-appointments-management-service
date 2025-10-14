import { NextRequest } from 'next/server';
import redirectToIdServer from '../../redirectToIdServer';
import { logInfo, logError } from '@services/logService';

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ provider: string }> },
) {
  const { provider } = await params;
  const redirectUrl = request.nextUrl.searchParams.get('redirectUrl') ?? '/';

  logInfo('Initiating login redirect', { provider, redirectUrl });

  try {
    if (!provider) {
      throw new Error('No provider specified');
    }

    await redirectToIdServer(redirectUrl, provider);
  } catch (e) {
    const error = e instanceof Error ? e : new Error(String(e));
    logError('No provider specified in route parameters', error, {
      provider,
      redirectUrl,
    });

    throw e;
  }
}
