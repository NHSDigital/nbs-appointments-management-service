'use server';

import { redirect } from 'next/navigation';
import { cookies } from 'next/headers';

async function redirectToIdServer(redirectUrl: string, authProvider: string) {
  const idServerEndpoint = `${process.env.AUTH_HOST}/api/authenticate?provider=${authProvider}`;
  cookies().set('previousPage', redirectUrl);

  return redirect(idServerEndpoint);
}

export default redirectToIdServer;
