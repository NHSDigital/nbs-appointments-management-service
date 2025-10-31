'use server';

import { redirect, RedirectType } from 'next/navigation';
import { cookies } from 'next/headers';
import { logInfo } from '@services/logService';
import { createHash } from 'crypto';
import { getProvider } from '@services/authProviderService';
import { AuthProvider } from '@types';

async function redirectToIdServer(redirectUrl: string, authProvider: string) {
  const cookieStore = await cookies();

  const provider = getProvider(authProvider);

  if (provider === undefined) {
    throw new Error(`Provider ${authProvider} not found`);
  }

  const idServerEndpoint = `${provider.HOST_URL}?${getQueryStringForProvider(provider)}`;

  cookieStore.set('previousPage', redirectUrl);

  logInfo('Redirecting to ID server', {
    redirectUrl,
    authProvider,
    idServerEndpoint,
  });

  return redirect(idServerEndpoint, RedirectType.push);
}

const getQueryStringForProvider = (provider: AuthProvider): URLSearchParams => {
  if (provider.IS_DIRECT) {
    const query = new URLSearchParams({
      client_id: provider.CLIENT_ID,
      redirect_uri: `http://localhost:3000/manage-your-appointments/auth/set-cookie-v2?provider=${provider.NAME}`,
      response_type: 'code',
      response_mode: 'query',
      code_challenge_method: 'S256',
      code_challenge: generateCodeChallenge(provider.CODE_CHALLENGE),
      scope: `openid profile email${provider.OFFLINE_ACCESS_ENABLED ? ' offline_access' : ''}`,
      prompt: 'login',
    });

    if (provider.REQUIRES_STATE_FOR_AUTHORIZE) {
      query.append('state', crypto.randomUUID());
    }

    return query;
  }

  return new URLSearchParams({
    provider: provider.NAME,
  });
};

const generateCodeChallenge = (codeVerifier: string): string => {
  const inputBytes = createHash('sha256').update(codeVerifier, 'utf8').digest();
  return inputBytes
    .toString('base64')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
};

export default redirectToIdServer;
