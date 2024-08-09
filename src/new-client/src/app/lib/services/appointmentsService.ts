'use server';
import { cookies } from 'next/headers';
import { revalidatePath } from 'next/cache';
import { redirect } from 'next/navigation';
import { Role, User, UserProfile } from '@types';
import { appointmentsApi } from '@services/api/appointmentsApi';

export const fetchAccessToken = async (code: string) => {
  const response = await appointmentsApi.post<{
    token: string;
  }>('token', code);
  return response.token;
};

export const fetchUserProfile = async () => {
  return appointmentsApi.get<UserProfile>('user/profile', {
    next: { tags: ['user'] },
  });
};

export async function fetchUsers(site: string) {
  const users = await appointmentsApi.get<User[]>(`users?site=${site}`, {
    cache: 'no-cache',
  });

  return users.filter(usr => usr.id.includes('@'));
}

export async function fetchRoles() {
  return await appointmentsApi.get<Role[]>('roles');
}

export async function fetchPermissions(site: string) {
  const response = await appointmentsApi.get<{ permissions: string[] }>(
    `user/permissions?site=${site}`,
  );

  return response?.permissions;
}

export const signOut = async () => {
  cookies().delete('token');
  revalidatePath('/');
  redirect('/');
};

export const saveUserRoleAssignments = async (
  site: string,
  user: string,
  roles: string[],
) => {
  const payload = {
    scope: `site:${site}`,
    user: user,
    roles: roles,
  };

  await nbsApi.post<string>(`user/roles`, JSON.stringify(payload));

  revalidatePath(`/site/${site}/users`);
  redirect(`/site/${site}/users`);
};
