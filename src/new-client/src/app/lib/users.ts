import { cookies } from 'next/headers';
import { getEndpoint } from './utils';
import { User } from '@types';

export async function fetchUsers(site: string) {
  const tokenCookie = cookies().get('token');

  const response = await fetch(getEndpoint(`users?site=${site}`), {
    cache: 'no-store',
    headers: {
      Authorization: `Bearer ${tokenCookie?.value}`,
    },
  });

  const users = (await response.json()) as User[];
  return users.filter(usr => usr.id.includes('@'));
}
