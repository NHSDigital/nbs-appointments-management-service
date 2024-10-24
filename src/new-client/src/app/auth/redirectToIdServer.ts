'use server';

import { redirect } from 'next/navigation';
import { headers, cookies } from 'next/headers';

async function redirectToIdServer(redirectUrl: string) {
  const idServerEndpoint = `${process.env.AUTH_HOST}/api/authenticate`;

  const host = headers().get('host');
  const protocol = host?.includes('localhost') ? 'http' : 'https';
  const routeHandlerToInvokeOnReturn = `${protocol}://${host}/auth/set-cookie`;

  cookies().set('previousPage', redirectUrl);

  return redirect(
    `${idServerEndpoint}?redirect_uri=${routeHandlerToInvokeOnReturn}`,
  );
}

export default redirectToIdServer;
