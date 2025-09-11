'use server';

import { redirect, RedirectType } from 'next/navigation';
import { cookies } from 'next/headers';
import { logEvent } from '../lib/logging/logger';

async function redirectToIdServer(redirectUrl: string, authProvider: string) {
  const cookieStore = await cookies();
  const idServerEndpoint = `${process.env.AUTH_HOST}/api/authenticate?provider=${authProvider}`;

  cookieStore.set('previousPage', redirectUrl);

  logEvent(
    'info',
    'Redirecting to ID server',
    { redirectUrl, authProvider, idServerEndpoint },
    'Auth/redirectToIdServer.ts',
  );

  return redirect(idServerEndpoint, RedirectType.push);
}

export default redirectToIdServer;
