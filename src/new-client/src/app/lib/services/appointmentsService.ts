'use server';
import { revalidatePath } from 'next/cache';
import { redirect } from 'next/navigation';
import { Role, User, UserProfile } from '@types';
import { appointmentsApi } from '@services/api/appointmentsApi';
import { ApiResponse } from '@types';

export const fetchAccessToken = async (code: string) => {
  const response = await appointmentsApi.post<{ token: string }>('token', code);
  return handleResponse(response);
};

export const fetchUserProfile = async () => {
  const response = await appointmentsApi.get<UserProfile>('user/profile', {
    next: { tags: ['user'] },
  });

  return handleResponse(response, undefined, true);
};

export async function fetchUsers(site: string) {
  const response = await appointmentsApi.get<User[]>(`users?site=${site}`, {
    cache: 'no-cache',
  });

  return (
    handleResponse(response, (users: User[]) =>
      users.filter(usr => usr.id.includes('@')),
    ) ?? []
  );
}

export const fetchSite = async (siteId: string) => {
  const userProfile = await fetchUserProfile();
  return userProfile?.availableSites.find(s => s.id === siteId);
};

export async function fetchRoles() {
  const response = await appointmentsApi.get<{ roles: Role[] }>(
    'roles?tag=canned',
  );

  return handleResponse(response)?.roles ?? [];
}

export async function fetchPermissions(site: string) {
  const response = await appointmentsApi.get<{ permissions: string[] }>(
    `user/permissions?site=${site}`,
  );

  return handleResponse(response)?.permissions ?? [];
}

function handleResponse<T>(
  response: ApiResponse<T>,
  transformData = (data: T) => data,
  suppress401Errors = false,
) {
  if (response.success) {
    if (response.data) return transformData(response.data);
    else return undefined;
  }

  if (response.httpStatusCode === 404) {
    return undefined;
  }

  if (response.httpStatusCode === 401 && suppress401Errors) {
    return undefined;
  }

  throw new Error(response.errorMessage);
}

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

  const response = await appointmentsApi.post(
    `user/roles`,
    JSON.stringify(payload),
  );

  handleResponse(response);
  revalidatePath(`/site/${site}/users`);
  redirect(`/site/${site}/users`);
};
