'use server';
import { UnauthorizedError } from '@types';
import { headers } from 'next/headers';
import { redirect } from 'next/navigation';

const notAuthorized = async () => {
  throw new UnauthorizedError();
};

const notAuthenticated = async () => {
  const lastRequestedPath = headers().get('mya-last-requested-path');

  redirect(
    lastRequestedPath ? `/login?redirectUrl=${lastRequestedPath}` : '/login',
  );
};

export { notAuthorized, notAuthenticated };
