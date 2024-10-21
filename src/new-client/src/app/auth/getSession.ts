'use server';

import { cookies } from 'next/headers';

type DecodedToken = {
  sub: string;
  // exp: number;
};

type Session = {
  emailAddress: string;
};

function getSession(): Session | undefined {
  const token = cookies().get('token')?.value;

  if (token === undefined) {
    return undefined;
  }

  const decodedToken = token
    ? JSON.parse(atob(token.split('.')[1]))
    : ({} as DecodedToken);

  return { emailAddress: decodedToken.sub };
}

function removeSession() {
  cookies().set('token', '', { expires: new Date(0) });
}

export { getSession, removeSession };
