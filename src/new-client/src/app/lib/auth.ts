'use server';

import { getEndpoint } from './utils';
import { UserProfile } from '@types';
import { revalidatePath } from 'next/cache';
import { redirect } from 'next/navigation';
import { cookies } from 'next/headers';

export async function fetchUserProfile() {
  const tokenCookie = cookies().get('token');

  if (tokenCookie) {
    const response = await fetch(getEndpoint('user/profile'), {
      headers: {
        Authorization: `Bearer ${tokenCookie.value}`,
      },
    });

    if (response.status === 200) {
      const json = await response.json();
      return json as UserProfile;
    }

    return undefined;
  }
}

export async function fetchAccessToken(code: string) {
  const response = await fetch(getEndpoint('token'), {
    method: 'POST',
    body: code,
  });

  if (response.status === 200) {
    const json = await response.json();
    if (json.token) {
      cookies().set('token', json.token);
    }
  }
}

export async function fecthPermissions(site: string) {
  const tokenCookie = cookies().get('token');

  if (tokenCookie) {
    const response = await fetch(getEndpoint(`user/permissions?site=${site}`), {
      headers: {
        Authorization: `Bearer ${tokenCookie.value}`,
      },
    });

    if (response.status === 200) {
      return response.json().then(data => data.permissions as string[]);
    }
  }

  return [] as string[];
}

export async function signOut() {
  cookies().delete('token');
  revalidatePath('/');
  redirect('/');
}
