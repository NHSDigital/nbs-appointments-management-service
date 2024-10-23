'use server';
import { revalidatePath } from 'next/cache';
import { notFound, redirect } from 'next/navigation';
import {
  AttributeDefinition,
  AttributeValue,
  Role,
  ApplyAvailabilityTemplateRequest,
  SiteWithAttributes,
  User,
  UserProfile,
} from '@types';
import { appointmentsApi } from '@services/api/appointmentsApi';
import { ApiResponse } from '@types';
import { raiseNotification } from '@services/notificationService';
import { cookies, headers } from 'next/headers';

export const fetchAccessToken = async (code: string) => {
  const response = await appointmentsApi.post<{ token: string }>('token', code);
  return handleBodyResponse(response);
};

export const fetchUserProfile = async (): Promise<UserProfile> => {
  const response = await appointmentsApi.get<UserProfile>('user/profile', {
    next: { tags: ['user'] },
  });

  return handleBodyResponse(response);
};

export async function fetchUsers(site: string) {
  const response = await appointmentsApi.get<User[]>(`users?site=${site}`, {
    cache: 'no-store',
  });

  return handleBodyResponse(response, (users: User[]) =>
    users.filter(usr => usr.id.includes('@')),
  );
}

export const fetchSite = async (siteId: string) => {
  const response = await appointmentsApi.get<SiteWithAttributes>(
    `sites/${siteId}`,
  );

  return handleBodyResponse(response);
};

export const fetchSiteAttributeValues = async (siteId: string) => {
  const response = await appointmentsApi.get<SiteWithAttributes>(
    `sites/${siteId}`,
  );

  return handleBodyResponse(response).attributeValues;
};

export async function fetchAttributeDefinitions() {
  const response = await appointmentsApi.get<AttributeDefinition[]>(
    'attributeDefinitions',
    {
      cache: 'force-cache',
    },
  );

  return handleBodyResponse(response);
}

export async function fetchRoles() {
  const response = await appointmentsApi.get<{ roles: Role[] }>(
    'roles?tag=canned',
  );

  return handleBodyResponse(response).roles;
}

export async function fetchPermissions(site: string) {
  const response = await appointmentsApi.get<{ permissions: string[] }>(
    `user/permissions?site=${site}`,
  );

  return handleBodyResponse(response).permissions;
}

function handleBodyResponse<T>(
  response: ApiResponse<T>,
  transformData = (data: T) => data,
): T {
  if (!response.success) {
    if (response.httpStatusCode === 404) {
      notFound();
    }

    if (response.httpStatusCode === 401) {
      const lastRequestedPath = headers().get('x-last-requested-path');

      const token = cookies().get('token');
      if (token) {
        cookies().delete('token');
      }

      redirect(
        lastRequestedPath
          ? `/login?redirectUrl=${lastRequestedPath}`
          : '/login',
      );
    }

    throw new Error(response.errorMessage);
  }

  if (!response.data) {
    throw new Error('A response body was expected but none was found.');
  }

  return transformData(response.data);
}

function handleEmptyResponse(response: ApiResponse<unknown>): void {
  if (response.success) {
    return;
  }

  if (response.httpStatusCode === 404) {
    notFound();
  }

  if (response.httpStatusCode === 401) {
    const lastRequestedPath = headers().get('x-last-requested-path');

    const token = cookies().get('token');
    if (token) {
      cookies().delete('token');
    }

    redirect(
      lastRequestedPath ? `/login?redirectUrl=${lastRequestedPath}` : '/login',
    );
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

  handleEmptyResponse(response);
  revalidatePath(`/site/${site}/users`);
  redirect(`/site/${site}/users`);
};

export const saveSiteAttributeValues = async (
  site: string,
  attributeValues: AttributeValue[],
) => {
  const response = await appointmentsApi.post(
    `sites/${site}/attributes`,
    JSON.stringify(attributeValues),
  );

  handleEmptyResponse(response);

  const notificationType = 'ams-notification';
  const notificationMessage =
    'You have successfully updated the access needs for the current site.';
  raiseNotification(notificationType, notificationMessage);

  revalidatePath(`/site/${site}/attributes`);
};

export const removeUserFromSite = async (site: string, user: string) => {
  const response = await appointmentsApi.post(
    `user/remove`,
    JSON.stringify({
      site,
      user,
    }),
  );

  handleEmptyResponse(response);

  const notificationType = 'ams-notification';
  const notificationMessage = `You have successfully removed ${user} from the current site.`;
  raiseNotification(notificationType, notificationMessage);

  revalidatePath(`/site/${site}/users`);
  redirect(`/site/${site}/users`);
};

export const saveAvailability = async (
  request: ApplyAvailabilityTemplateRequest,
) => {
  const response = await appointmentsApi.post(
    `availability/apply-template`,
    JSON.stringify(request),
  );

  handleResponse(response);

  const notificationType = 'ams-notification';
  const notificationMessage =
    'You have successfully created availability for the current site.';
  raiseNotification(notificationType, notificationMessage);

  // TODO: Once the fetch availability route is implemented, refresh the tag here
  // revalidateTag(`fetchAvailability`);
};
