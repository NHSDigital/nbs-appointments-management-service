'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';
import { cookies } from 'next/headers';
import { fetchUserProfile } from '@services/appointmentsService';
import { revalidateTag } from 'next/cache';
import fromServer from '@server/fromServer';
import { getProvider } from '@services/authProviderService';

export async function GET(request: NextRequest) {
  const code = request.nextUrl.searchParams.get('code');
  const provider = request.nextUrl.searchParams.get('provider');
  const cookieStore = await cookies();

  if (code === null || code === '') {
    throw Error('No code found in request');
  }

  if (provider === null || provider === '') {
    throw Error('No provider found in request');
  }

  const authProvider = getProvider(provider);

  if (authProvider === undefined) {
    throw new Error(`Provider ${provider} not found`);
  }

  const tokenResponse = await fetch(authProvider.TOKEN_URL, {
    method: 'POST',
    body: new URLSearchParams({
      client_id: authProvider.CLIENT_ID,
      code: code,
      redirect_uri: `http://localhost:3000/manage-your-appointments/auth/set-cookie-v2?provider=${authProvider.NAME}`,
      scope: `openid profile email${authProvider.OFFLINE_ACCESS_ENABLED ? ' offline_access' : ''}`,
      grant_type: 'authorization_code',
      code_verifier: authProvider.CODE_CHALLENGE,
      client_secret: authProvider.CLIENT_SECRET,
    }),
  });

  const token = await tokenResponse.json();

  cookieStore.set('token', token.id_token);
  revalidateTag('user');

  // Implicitly checks EULA is accepted, handles if not
  await fromServer(fetchUserProfile(`${process.env.CLIENT_BASE_PATH}/eula`));

  const previousPage = cookieStore.get('previousPage');
  if (previousPage) {
    cookieStore.delete('previousPage');
    redirect(`${process.env.CLIENT_BASE_PATH}/${previousPage.value}`);
  }

  redirect(`${process.env.CLIENT_BASE_PATH}/sites`);
}
