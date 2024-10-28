'use server';
import { headers } from 'next/headers';
import { redirect } from 'next/navigation';

const notAuthorised = () => {
  throw new Error('Forbidden: You lack the necessary permissions');
};

const notAuthenticated = () => {
  const lastRequestedPath = headers().get('mya-last-requested-path');

  redirect(
    lastRequestedPath ? `/login?redirectUrl=${lastRequestedPath}` : '/login',
  );
};

export { notAuthorised, notAuthenticated };
