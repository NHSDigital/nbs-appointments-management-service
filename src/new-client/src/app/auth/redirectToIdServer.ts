'use server';

import { redirect } from 'next/navigation';
import { headers, cookies } from 'next/headers';

async function redirectToIdServer(currentPage?: string) {
  const idServerEndpoint = `${process.env.AUTH_HOST}/api/authenticate`;

  const host = headers().get('host');
  const protocol = host?.includes('localhost') ? 'http' : 'https';
  const routeHandlerToInvokeOnReturn = `${protocol}://${host}/auth/set-cookie`;

  const previousPage = currentPage ?? headers().get('referer');
  if (previousPage !== null) {
    cookies().set('previousPage', previousPage);
  }

  return redirect(
    `${idServerEndpoint}?redirect_uri=${routeHandlerToInvokeOnReturn}`,
  );
}

export default redirectToIdServer;
