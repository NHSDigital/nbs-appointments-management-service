'use server';
import { revalidatePath, revalidateTag } from 'next/cache';
import { notFound, redirect } from 'next/navigation';
import {
  AttributeDefinition,
  Role,
  SetAttributesRequest,
  ApplyAvailabilityTemplateRequest,
  SiteWithAttributes,
  User,
  UserProfile,
  SetAvailabilityRequest,
  AvailabilityCreatedEvent,
  FetchAvailabilityRequest,
  AvailabilityResponse,
  FetchBookingsRequest,
  Booking,
  DailyAvailability,
  EulaVersion,
  WellKnownOdsEntry,
  EditSessionRequest,
  CancelSessionRequest,
  SessionSummary,
  Site,
} from '@types';
import { appointmentsApi } from '@services/api/appointmentsApi';
import { ApiResponse } from '@types';
import { raiseNotification } from '@services/notificationService';
import { notAuthenticated, notAuthorized } from '@services/authService';
import { now } from '@services/timeService';
import dayjs from 'dayjs';

export const fetchAccessToken = async (code: string) => {
  const response = await appointmentsApi.post<{ token: string }>('token', code);
  return handleBodyResponse(response);
};

export const fetchUserProfile = async (
  eulaRoute = '/eula',
): Promise<UserProfile> => {
  const response = await appointmentsApi.get<UserProfile>('user/profile', {
    next: { tags: ['user'] },
  });

  const userProfile = handleBodyResponse(response);
  await assertEulaAcceptance(userProfile, eulaRoute);
  return userProfile;
};

export const fetchSitesPreview = async (): Promise<Site[]> => {
  const response = await appointmentsApi.get<Site[]>('sites-preview', {
    next: { tags: ['user'] },
  });

  return handleBodyResponse(response);
};

export const assertEulaAcceptance = async (
  userProfile: UserProfile,
  eulaRoute = '/eula',
) => {
  if (userProfile.hasSites) {
    const eulaVersion = await fetchEula();

    if (eulaVersion.versionDate !== userProfile.latestAcceptedEulaVersion) {
      redirect(eulaRoute);
    }
  }
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
    `sites/${siteId}?scope=*`,
    {
      next: { tags: ['site'] },
    },
  );
  return handleBodyResponse(response);
};

export const fetchSiteAttributeValues = async (siteId: string) => {
  const response = await appointmentsApi.get<SiteWithAttributes>(
    `sites/${siteId}?scope=*`,
  );

  return handleBodyResponse(response)?.attributeValues ?? [];
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

export async function fetchWellKnownOdsCodeEntries() {
  const response = await appointmentsApi.get<WellKnownOdsEntry[]>(
    'wellKnownOdsCodeEntries',
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

export async function fetchAvailabilityCreatedEvents(site: string) {
  const response = await appointmentsApi.get<AvailabilityCreatedEvent[]>(
    `availability-created?site=${site}&from=${now().format('YYYY-MM-DD')}`,
    {
      next: { tags: ['availability-created'] },
    },
  );

  return handleBodyResponse(response);
}

export async function fetchEula() {
  const response = await appointmentsApi.get<EulaVersion>('eula', {
    next: { revalidate: 60 * 60 * 24 },
  });
  return handleBodyResponse(response);
}

export async function acceptEula(versionDate: string) {
  const payload = {
    versionDate,
  };

  const response = await appointmentsApi.post(
    `eula/consent`,
    JSON.stringify(payload),
  );

  handleEmptyResponse(response);
  revalidatePath(`eula`);
}

export async function assertPermission(site: string, permission: string) {
  const response = await fetchPermissions(site);

  if (!response.includes(permission)) {
    notAuthorized();
  }
}

export async function assertAnyPermissions(
  site: string,
  permissions: string[],
) {
  const response = await fetchPermissions(site);

  if (!permissions.some(permission => response.includes(permission))) {
    notAuthorized();
  }
}

export async function assertAllPermissions(
  site: string,
  permissions: string[],
) {
  const response = await fetchPermissions(site);

  if (!permissions.every(permission => response.includes(permission))) {
    notAuthorized();
  }
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
      notAuthenticated();
    }

    if (response.httpStatusCode === 403) {
      notAuthorized();
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
    notAuthenticated();
  }

  if (response.httpStatusCode === 403) {
    notAuthorized();
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
  attributeValues: SetAttributesRequest,
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

export const applyAvailabilityTemplate = async (
  request: ApplyAvailabilityTemplateRequest,
) => {
  const response = await appointmentsApi.post(
    `availability/apply-template`,
    JSON.stringify(request),
  );

  handleEmptyResponse(response);

  revalidateTag('availability-created');

  const notificationType = 'ams-notification';
  const notificationMessage =
    'You have successfully created availability for the current site.';
  raiseNotification(notificationType, notificationMessage);

  revalidateTag(`fetchAvailability`);
};

export const saveAvailability = async (request: SetAvailabilityRequest) => {
  const response = await appointmentsApi.post(
    `availability`,
    JSON.stringify(request),
  );

  handleEmptyResponse(response);

  revalidateTag('availability-created');

  const notificationType = 'ams-notification';
  const notificationMessage =
    'You have successfully created availability for the current site.';
  raiseNotification(notificationType, notificationMessage);

  revalidateTag(`fetchAvailability`);
};

export const editSession = async (request: EditSessionRequest) => {
  const response = await appointmentsApi.post(
    `availability`,
    JSON.stringify(request),
  );

  handleEmptyResponse(response);

  revalidateTag('availability-created');

  const notificationType = 'ams-notification';
  const notificationMessage = 'You have successfully edited the session.';
  raiseNotification(notificationType, notificationMessage);

  revalidateTag(`fetchAvailability`);
};

export async function fetchInformationForCitizens(site: string, scope: string) {
  const response = await appointmentsApi.get<SiteWithAttributes>(
    `sites/${site}?scope=${scope}`,
  );

  return handleBodyResponse(response)?.attributeValues ?? [];
}

export const setSiteInformationForCitizen = async (
  site: string,
  attributeValues: SetAttributesRequest,
) => {
  const response = await appointmentsApi.post(
    `sites/${site}/attributes`,
    JSON.stringify(attributeValues),
  );

  const notificationType = 'ams-notification';
  const notificationMessage =
    "You have successfully updated the current site's information.";
  raiseNotification(notificationType, notificationMessage);

  handleEmptyResponse(response);
  revalidatePath(`/site/${site}/details`);
};

export const fetchAvailability = async (payload: FetchAvailabilityRequest) => {
  const response = await appointmentsApi.post<AvailabilityResponse[]>(
    'availability/query',
    JSON.stringify(payload),
    {
      next: { tags: ['fetchAvailability'] },
    },
  );

  return handleBodyResponse(response);
};

export const fetchBookings = async (payload: FetchBookingsRequest) => {
  const response = await appointmentsApi.post<Booking[]>(
    'booking/query',
    JSON.stringify(payload),
  );

  return handleBodyResponse(response);
};

export const fetchDailyAvailability = async (
  site: string,
  from: string,
  until: string,
) => {
  const response = await appointmentsApi.get<DailyAvailability[]>(
    `daily-availability?site=${site}&from=${from}&until=${until}`,
  );

  return handleBodyResponse(response);
};

export const fetchBooking = async (reference: string, site: string) => {
  const response = await appointmentsApi.get<Booking>(
    `booking/${reference}?site=${site}`,
  );

  return handleBodyResponse(response);
};

export const cancelAppointment = async (reference: string, site: string) => {
  const response = await appointmentsApi.post(
    `booking/${reference}/cancel?site=${site}`,
  );

  return handleEmptyResponse(response);
};

export const cancelSession = async (
  sessionSummary: SessionSummary,
  site: string,
) => {
  const payload: CancelSessionRequest = {
    site: site,
    date: dayjs(sessionSummary.start).format('YYYY-MM-DD'),
    from: dayjs(sessionSummary.start).format('HH:mm'),
    until: dayjs(sessionSummary.end).format('HH:mm'),
    services: Object.keys(sessionSummary.bookings),
    capacity: sessionSummary.capacity,
    slotLength: sessionSummary.slotLength,
  };

  const response = await appointmentsApi.post(
    'session/cancel',
    JSON.stringify(payload),
  );

  return handleEmptyResponse(response);
};
