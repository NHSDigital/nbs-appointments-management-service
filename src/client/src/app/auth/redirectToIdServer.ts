'use server';

import { redirect } from 'next/navigation';
import { cookies } from 'next/headers';

async function redirectToIdServer(redirectUrl: string) {
  const idServerEndpoint = `${process.env.AUTH_HOST}/api/authenticate`;
  cookies().set('previousPage', redirectUrl);

  return redirect(idServerEndpoint);
}

export default redirectToIdServer;
