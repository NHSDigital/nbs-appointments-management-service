import { NextRequest } from 'next/server';
import redirectToIdServer from '../../redirectToIdServer';

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ provider: string }> },
) {
  const { provider } = await params;
  const redirectUrl = request.nextUrl.searchParams.get('redirectUrl') ?? '/';

  await redirectToIdServer(redirectUrl, provider);
}
