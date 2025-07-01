'use server';
import { revalidatePath, revalidateTag } from 'next/cache';
import { notFound, redirect } from 'next/navigation';
import {
  AccessibilityDefinition,
  Role,
  SetAccessibilitiesRequest,
  ApplyAvailabilityTemplateRequest,
  User,
  UserProfile,
  SetAvailabilityRequest,
  AvailabilityCreatedEvent,
  FetchBookingsRequest,
  Booking,
  DailyAvailability,
  EulaVersion,
  WellKnownOdsEntry,
  EditSessionRequest,
  CancelSessionRequest,
  SessionSummary,
  SetSiteDetailsRequest,
  SetInformationForCitizensRequest,
  Site,
  SetSiteReferenceDetailsRequest,
  FeatureFlag,
  clinicalServices,
  BookingStatus,
  UserIdentityStatus,
  WeekSummaryV2,
} from '@types';
import { appointmentsApi } from '@services/api/appointmentsApi';
import { ApiResponse, ClinicalService } from '@types';
import { raiseNotification } from '@services/notificationService';
import { notAuthenticated, notAuthorized } from '@services/authService';
import {
  dateFormat,
  dateTimeFormat,
  parseToUkDatetime,
  ukNow,
} from '@services/timeService';

export const fetchAccessToken = async (code: string, provider: string) => {
  const response = await appointmentsApi.post<{ token: string }>(
    `token?provider=${provider}`,
    code,
  );
  return handleBodyResponse(response);
};

export const fetchUserProfile = async (
  eulaRoute = '/eula',
): Promise<UserProfile> => {
  const response = await appointmentsApi.get<UserProfile>('user/profile', {
    next: { tags: ['user'] },
  });

  const userProfile = await handleBodyResponse(response);
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

export async function proposeNewUser(siteId: string, userId: string) {
  const payload = {
    siteId,
    userId,
  };

  const response = await appointmentsApi.post<UserIdentityStatus>(
    `user/propose-potential`,
    JSON.stringify(payload),
  );

  return handleBodyResponse(response);
}

export async function fetchUsers(site: string) {
  const response = await appointmentsApi.get<User[]>(`users?site=${site}`, {
    cache: 'no-store',
    next: { tags: ['user'] },
  });

  return handleBodyResponse(response, (users: User[]) =>
    users.filter(usr => usr.id.includes('@')),
  );
}

export const fetchSite = async (siteId: string) => {
  const response = await appointmentsApi.get<Site>(`sites/${siteId}?scope=*`, {
    next: { tags: ['site'] },
  });
  return handleBodyResponse(response);
};

export const fetchFeatureFlag = async (featureFlag: string) => {
  const response = await appointmentsApi.get<FeatureFlag>(
    `feature-flag/${featureFlag}`,
  );
  return handleBodyResponse(response);
};

export const fetchClinicalServices = async () => {
  const canUseMultipleServices = await fetchFeatureFlag('MultipleServices');

  if (canUseMultipleServices.enabled) {
    const response = await appointmentsApi.get<ClinicalService[]>(
      `clinical-services`,
      {
        cache: 'force-cache',
      },
    );
    return handleBodyResponse(response);
  }

  return clinicalServices;
};

export const fetchSiteAccessibilities = async (siteId: string) => {
  const response = await appointmentsApi.get<Site>(`sites/${siteId}?scope=*`);

  return (await handleBodyResponse(response))?.accessibilities ?? [];
};

export async function fetchAccessibilityDefinitions() {
  const response = await appointmentsApi.get<AccessibilityDefinition[]>(
    'accessibilityDefinitions',
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

  return (await handleBodyResponse(response)).roles;
}

export async function fetchPermissions(site: string) {
  const response = await appointmentsApi.get<{ permissions: string[] }>(
    `user/permissions?site=${site}`,
  );

  return (await handleBodyResponse(response)).permissions;
}

export async function fetchAvailabilityCreatedEvents(site: string) {
  const response = await appointmentsApi.get<AvailabilityCreatedEvent[]>(
    `availability-created?site=${site}&from=${ukNow().format(dateFormat)}`,
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

async function handleBodyResponse<T>(
  response: ApiResponse<T>,
  transformData = (data: T) => data,
): Promise<T> {
  if (!response.success) {
    if (response.httpStatusCode === 404) {
      notFound();
    }

    if (response.httpStatusCode === 401) {
      await notAuthenticated();
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

async function handleEmptyResponse(
  response: ApiResponse<unknown>,
): Promise<void> {
  if (response.success) {
    return;
  }

  if (response.httpStatusCode === 404) {
    notFound();
  }

  if (response.httpStatusCode === 401) {
    await notAuthenticated();
  }

  if (response.httpStatusCode === 403) {
    notAuthorized();
  }

  throw new Error(response.errorMessage);
}

export const saveUserRoleAssignments = async (
  site: string,
  user: string,
  firstName: string,
  lastName: string,
  roles: string[],
) => {
  const payload = {
    scope: `site:${site}`,
    user: user,
    firstName,
    lastName,
    roles: roles,
  };

  const response = await appointmentsApi.post(
    `user/roles`,
    JSON.stringify(payload),
  );
  handleEmptyResponse(response);

  // re-implement in https://nhsd-jira.digital.nhs.uk/browse/APPT-799
  // const notificationType = 'ams-notification';
  // const notificationMessage = isEdit
  //   ? `You have changed a user's role.`
  //   : `You have added a new user to MYA The user will be sent information about how to login.`;
  // raiseNotification(notificationType, notificationMessage);

  revalidateTag('users');
  revalidatePath(`/site/${site}/users`);
  redirect(`/site/${site}/users`);
};

export const saveSiteAccessibilities = async (
  site: string,
  accessibilities: SetAccessibilitiesRequest,
) => {
  const response = await appointmentsApi.post(
    `sites/${site}/accessibilities`,
    JSON.stringify(accessibilities),
  );

  handleEmptyResponse(response);

  const notificationType = 'ams-notification';
  const notificationMessage =
    'You have successfully updated the access needs for the current site.';
  await raiseNotification(notificationType, notificationMessage);

  revalidatePath(`/site/${site}/accessibilities`);
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
  await raiseNotification(notificationType, notificationMessage);

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
  await raiseNotification(notificationType, notificationMessage);

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
  await raiseNotification(notificationType, notificationMessage);

  revalidateTag(`fetchAvailability`);
};

export async function fetchInformationForCitizens(site: string) {
  const response = await appointmentsApi.get<Site>(`sites/${site}`);

  return (await handleBodyResponse(response))?.informationForCitizens ?? '';
}

export const setSiteInformationForCitizen = async (
  site: string,
  informationForCitizens: SetInformationForCitizensRequest,
) => {
  const response = await appointmentsApi.post(
    `sites/${site}/informationForCitizens`,
    JSON.stringify(informationForCitizens),
  );

  const notificationType = 'ams-notification';
  const notificationMessage =
    "You have successfully updated the current site's information.";
  await raiseNotification(notificationType, notificationMessage);

  handleEmptyResponse(response);
  revalidatePath(`/site/${site}/details`);
};

export const fetchBookings = async (
  payload: FetchBookingsRequest,
  statuses: BookingStatus[],
) => {
  const response = await appointmentsApi.post<Booking[]>(
    'booking/query',
    JSON.stringify(payload),
  );

  const bookings = await handleBodyResponse(response);

  return bookings.filter(b => statuses.includes(b.status));
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

export const fetchWeekSummaryV2 = async (site: string, from: string) => {
  const response = await appointmentsApi.get<WeekSummaryV2>(
    `week-summary?site=${site}&from=${from}`,
  );

  return handleBodyResponse(response);
};

export const fetchBooking = async (reference: string, site: string) => {
  const response = await appointmentsApi.get<Booking>(
    `booking/${reference}?site=${site}`,
  );

  return handleBodyResponse(response);
};

export const cancelAppointment = async (
  reference: string,
  site: string,
  cancellationReason: string,
) => {
  const response = await appointmentsApi.post(
    `booking/${reference}/cancel?site=${site}&cancellationReason=${cancellationReason}`,
  );

  return handleEmptyResponse(response);
};

export const saveSiteDetails = async (
  site: string,
  details: SetSiteDetailsRequest,
) => {
  const response = await appointmentsApi.post(
    `sites/${site}/details`,
    JSON.stringify(details),
  );

  handleEmptyResponse(response);

  const notificationType = 'ams-notification';
  const notificationMessage =
    'You have successfully updated the details for the current site.';
  await raiseNotification(notificationType, notificationMessage);
};

export const saveSiteReferenceDetails = async (
  site: string,
  referenceDetails: SetSiteReferenceDetailsRequest,
) => {
  const response = await appointmentsApi.post(
    `sites/${site}/reference-details`,
    JSON.stringify(referenceDetails),
  );
  handleEmptyResponse(response);

  const notificationType = 'ams-notification';
  const notificationMessage =
    'You have successfully updated the reference details for the current site.';
  raiseNotification(notificationType, notificationMessage);
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
  await raiseNotification(notificationType, notificationMessage);

  revalidateTag(`fetchAvailability`);
};

export const cancelSession = async (
  sessionSummary: SessionSummary,
  site: string,
) => {
  const ukStartDatetime = parseToUkDatetime(
    sessionSummary.ukStartDatetime,
    dateTimeFormat,
  );
  const ukEndDatetime = parseToUkDatetime(
    sessionSummary.ukEndDatetime,
    dateTimeFormat,
  );
  const payload: CancelSessionRequest = {
    site: site,
    date: ukStartDatetime.format(dateFormat),
    from: ukStartDatetime.format('HH:mm'),
    until: ukEndDatetime.format('HH:mm'),
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
