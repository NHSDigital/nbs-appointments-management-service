'use server';
import { cookies } from 'next/headers';
import { getEndpoint } from './utils';
import { User } from '@types';
import { revalidatePath } from 'next/cache';
import { redirect } from 'next/navigation';

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

export async function saveUserRoleAssignments(
  site: string,
  user: string,
  roles: string[],
) {
  const tokenCookie = cookies().get('token');

  const payload = {
    scope: `site:${site}`,
    user: user,
    roles: roles,
  };

  const response = await fetch(getEndpoint('user/roles'), {
    method: 'POST',
    body: JSON.stringify(payload),
    headers: {
      Authorization: `Bearer ${tokenCookie?.value}`,
    },
  });

  if (response.status === 200) {
    revalidatePath(`/site/${site}/users`);
    redirect(`/site/${site}/users`);
  } else {
    throw Error('There was a problem saving the user assignments');
  }
}
