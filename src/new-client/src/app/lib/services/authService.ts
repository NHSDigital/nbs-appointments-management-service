'use server';
import { UnauthorisedError } from '@types';
import { headers } from 'next/headers';
import { redirect } from 'next/navigation';

const notAuthorised = () => {
  throw new UnauthorisedError();
};

const notAuthenticated = () => {
  const lastRequestedPath = headers().get('mya-last-requested-path');

  redirect(
    lastRequestedPath ? `/login?redirectUrl=${lastRequestedPath}` : '/login',
  );
};

export { notAuthorised, notAuthenticated };
