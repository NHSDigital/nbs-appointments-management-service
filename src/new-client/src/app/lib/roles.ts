import { cookies } from 'next/headers';
import { getEndpoint } from './utils';
import { Role } from '@types';

export async function fetchRoles() {
  const tokenCookie = cookies().get('token');

  const response = await fetch(getEndpoint('roles?tag=canned'), {
    headers: {
      Authorization: `Bearer ${tokenCookie?.value}`,
    },
  });

  const json = await response.json();
  return json.roles as Role[];
}
