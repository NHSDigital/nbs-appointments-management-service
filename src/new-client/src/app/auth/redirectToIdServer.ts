'use server';

import { redirect } from 'next/navigation';
import { headers, cookies } from 'next/headers';

async function redirectToIdServer(currentPage?: string) {
  const idServerEndpoint = `${process.env.NBS_API_BASE_URL}/api/authenticate`;

  const routeHandlerToInvokeOnReturn = `${process.env.NBS_CLIENT_BASE_URL}/auth/set-cookie`;

  const previousPage = currentPage ?? headers().get('referer');
  if (previousPage !== null) {
    cookies().set('previousPage', previousPage);
  }

  return redirect(
    `${idServerEndpoint}?redirect_uri=${routeHandlerToInvokeOnReturn}`,
  );
}

export default redirectToIdServer;
