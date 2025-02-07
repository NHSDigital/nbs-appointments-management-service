'use server';

import { redirect, RedirectType } from 'next/navigation';
import { cookies } from 'next/headers';

async function redirectToIdServer(redirectUrl: string, authProvider: string) {
  const cookieStore = await cookies();
  const idServerEndpoint = `${process.env.AUTH_HOST}/api/authenticate?provider=${authProvider}`;
  cookieStore.set('previousPage', redirectUrl);

  return redirect(idServerEndpoint, RedirectType.push);
}

export default redirectToIdServer;
