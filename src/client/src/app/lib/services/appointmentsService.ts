'use server';
import { revalidatePath, revalidateTag } from 'next/cache';
import { notFound, redirect } from 'next/navigation';
import {
  AccessibilityDefinition,
  ApplyAvailabilityTemplateRequest,
  AvailabilityChangeProposalRequest,
  AvailabilityChangeProposalResponse,
  AvailabilityCreatedEvent,
  Booking,
  BookingStatus,
  CancelDayRequest,
  CancelDayResponse,
  CancelSessionRequest,
  DailyAvailability,
  DaySummaryV2,
  EditSessionRequest,
  EulaVersion,
  FeatureFlag,
  FetchBookingsRequest,
  Role,
  ServerActionResult,
  SessionSummary,
  SetAccessibilitiesRequest,
  SetAvailabilityRequest,
  SetInformationForCitizensRequest,
  SetSiteDetailsRequest,
  SetSiteReferenceDetailsRequest,
  Site,
  SiteStatus,
  UpdateSiteStatusRequest,
  User,
  UserIdentityStatus,
  UserProfile,
  WeekSummaryV2,
  WellKnownOdsEntry,
} from '@types';
import { appointmentsApi } from '@services/api/appointmentsApi';
import { ApiResponse, ClinicalService } from '@types';
import { raiseNotification } from '@services/notificationService';
import { notAuthenticated, notAuthorized } from '@services/authService';
import {
  RFC3339Format,
  dateTimeFormat,
  parseToUkDatetime,
  ukNow,
} from '@services/timeService';

export const fetchAccessToken = async (
  code: string,
  provider: string,
): Promise<ServerActionResult<string>> =>
  appointmentsApi
    .post<{ token: string }>(`token?provider=${provider}`, code)
    .then(response => handleBodyResponse(response, data => data.token));

export const fetchUserProfile = async (
  eulaRoute = '/eula',
): Promise<ServerActionResult<UserProfile>> =>
  appointmentsApi
    .get<UserProfile>('user/profile', {
      next: { tags: ['user'] },
    })
    .then(userProfileResponse => handleBodyResponse(userProfileResponse))
    .then(async userProfileResponse => {
      if (userProfileResponse.success) {
        await assertEulaAcceptance(userProfileResponse.data, eulaRoute);
      }
      return userProfileResponse;
    });

export const fetchSitesPreview = async (): Promise<
  ServerActionResult<Site[]>
> =>
  appointmentsApi
    .get<Site[]>('sites-preview', {
      next: { tags: ['user'] },
    })
    .then(response => handleBodyResponse(response));

export const assertEulaAcceptance = async (
  userProfile: UserProfile,
  eulaRoute = '/eula',
) => {
  if (userProfile.hasSites) {
    const eulaVersionResponse = await fetchEula();

    if (!eulaVersionResponse.success) {
      return Promise.reject('Failed to fetch EULA version');
    }

    if (
      eulaVersionResponse.data.versionDate !==
      userProfile.latestAcceptedEulaVersion
    ) {
      redirect(eulaRoute);
    }
  }
};

export const proposeNewUser = async (
  siteId: string,
  userId: string,
): Promise<ServerActionResult<UserIdentityStatus>> =>
  appointmentsApi
    .post<UserIdentityStatus>(
      `user/propose-potential`,
      JSON.stringify({
        siteId,
        userId,
      }),
    )
    .then(response => handleBodyResponse(response));

export const fetchUsers = async (
  site: string,
): Promise<ServerActionResult<User[]>> =>
  appointmentsApi
    .get<User[]>(`users?site=${site}`, {
      cache: 'no-store',
      next: { tags: ['user'] },
    })
    .then(response =>
      handleBodyResponse(response, (users: User[]) =>
        users.filter(usr => usr.id.includes('@')),
      ),
    );

export const fetchSite = async (
  siteId: string,
): Promise<ServerActionResult<Site>> =>
  appointmentsApi
    .get<Site>(`sites/${siteId}?scope=*`, {
      next: { tags: ['site'] },
    })
    .then(response => handleBodyResponse(response));

export const fetchFeatureFlag = async (
  featureFlag: string,
): Promise<ServerActionResult<FeatureFlag>> =>
  appointmentsApi
    .get<FeatureFlag>(`feature-flag/${featureFlag}`)
    .then(response => handleBodyResponse(response));

export const fetchClinicalServices = async (): Promise<
  ServerActionResult<ClinicalService[]>
> =>
  appointmentsApi
    .get<ClinicalService[]>(`clinical-services`, {
      cache: 'force-cache',
    })
    .then(response => handleBodyResponse(response));

export const fetchAccessibilityDefinitions = async (): Promise<
  ServerActionResult<AccessibilityDefinition[]>
> =>
  appointmentsApi
    .get<AccessibilityDefinition[]>('accessibilityDefinitions', {
      cache: 'force-cache',
    })
    .then(response => handleBodyResponse(response));

export const fetchWellKnownOdsCodeEntries = async (): Promise<
  ServerActionResult<WellKnownOdsEntry[]>
> =>
  appointmentsApi
    .get<WellKnownOdsEntry[]>('wellKnownOdsCodeEntries', {
      cache: 'force-cache',
    })
    .then(response => handleBodyResponse(response));

export const fetchRoles = async (): Promise<ServerActionResult<Role[]>> =>
  appointmentsApi
    .get<{ roles: Role[] }>('roles?tag=canned')
    .then(response => handleBodyResponse(response, data => data.roles));

export const fetchPermissions = async (
  site: string | undefined,
): Promise<ServerActionResult<string[]>> => {
  if (!site) {
    return { success: true, data: [] };
  }

  return appointmentsApi
    .get<{ permissions: string[] }>(`user/permissions?site=${site}`)
    .then(response => handleBodyResponse(response, data => data.permissions));
};

export const fetchAvailabilityCreatedEvents = async (
  site: string,
): Promise<ServerActionResult<AvailabilityCreatedEvent[]>> =>
  appointmentsApi
    .get<AvailabilityCreatedEvent[]>(
      `availability-created?site=${site}&from=${ukNow().format(RFC3339Format)}`,
      {
        next: { tags: ['availability-created'] },
      },
    )
    .then(handleBodyResponse);

export const fetchEula = async (): Promise<ServerActionResult<EulaVersion>> =>
  appointmentsApi
    .get<EulaVersion>('eula', {
      next: { revalidate: 60 * 60 * 24 },
    })
    .then(handleBodyResponse);

export const acceptEula = async (
  versionDate: string,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(
      `eula/consent`,
      JSON.stringify({
        versionDate,
      }),
    )
    .then(handleEmptyResponse)
    .then(response => {
      revalidatePath(`eula`);
      return response;
    });

export const assertPermission = async (
  site: string,
  permission: string,
): Promise<ServerActionResult<void>> => {
  const response = await fetchPermissions(site);
  if (!response.success) {
    return Promise.reject('Failed to fetch permissions');
  }

  if (!response.data.includes(permission)) {
    notAuthorized();
  }

  return { success: true, data: undefined };
};

export const assertFeatureEnabled = async (
  flag: string,
): Promise<ServerActionResult<void>> => {
  const response = await fetchFeatureFlag(flag);
  if (!response.success) {
    return Promise.reject('Failed to fetch feature flag');
  }

  if (!response.data.enabled) {
    notFound();
  }

  return { success: true, data: undefined };
};

export const assertAnyPermissions = async (
  site: string,
  permissions: string[],
): Promise<ServerActionResult<void>> => {
  const response = await fetchPermissions(site);
  if (!response.success) {
    return Promise.reject('Failed to fetch permissions');
  }

  if (!permissions.some(permission => response.data.includes(permission))) {
    notAuthorized();
  }

  return { success: true, data: undefined };
};

export const assertAllPermissions = async (
  site: string,
  permissions: string[],
): Promise<ServerActionResult<void>> => {
  const response = await fetchPermissions(site);
  if (!response.success) {
    return Promise.reject('Failed to fetch permissions');
  }

  if (!permissions.every(permission => response.data.includes(permission))) {
    notAuthorized();
  }

  return { success: true, data: undefined };
};

async function handleBodyResponse<T, Y = T>(
  response: ApiResponse<T>,
  transformData = (data: T) => data as unknown as Y,
): Promise<ServerActionResult<Y>> {
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

    return Promise.reject(
      `Response code ${response.httpStatusCode} did not indicate success.`,
    );
  }

  if (!response.data) {
    return Promise.reject(`Expected data in response, but found none.`);
  }

  return { success: true, data: transformData(response.data) };
}

async function handleEmptyResponse(
  response: ApiResponse<unknown>,
): Promise<ServerActionResult<void>> {
  if (response.success) {
    return { success: true, data: undefined };
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

  return Promise.reject(
    `Response code ${response.httpStatusCode} did not indicate success.`,
  );
}

export const saveUserRoleAssignments = async (
  site: string,
  user: string,
  firstName: string,
  lastName: string,
  roles: string[],
): Promise<ServerActionResult<void>> => {
  const payload = {
    scope: `site:${site}`,
    user: user,
    firstName,
    lastName,
    roles: roles,
  };

  return appointmentsApi
    .post(`user/roles`, JSON.stringify(payload))
    .then(handleEmptyResponse)
    .then(_ => {
      // re-implement in https://nhsd-jira.digital.nhs.uk/browse/APPT-799
      // const notificationType = 'ams-notification';
      // const notificationMessage = isEdit
      //   ? `You have changed a user's role.`
      //   : `You have added a new user to MYA The user will be sent information about how to login.`;
      // raiseNotification(notificationType, notificationMessage);

      revalidateTag('users');
      revalidatePath(`/site/${site}/users`);
      redirect(`/site/${site}/users`);
    });
};

export const saveSiteAccessibilities = async (
  site: string,
  accessibilities: SetAccessibilitiesRequest,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(`sites/${site}/accessibilities`, JSON.stringify(accessibilities))
    .then(handleEmptyResponse)
    .then(async _ => {
      const notificationType = 'ams-notification';
      const notificationMessage =
        'You have successfully updated the access needs for the current site.';
      await raiseNotification(notificationType, notificationMessage);

      revalidatePath(`/site/${site}/accessibilities`);
      return { success: true, data: undefined };
    });

export const removeUserFromSite = async (
  site: string,
  user: string,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(
      `user/remove`,
      JSON.stringify({
        site,
        user,
      }),
    )
    .then(handleEmptyResponse)
    .then(async _ => {
      const notificationType = 'ams-notification';
      const notificationMessage = `You have successfully removed ${user} from the current site.`;
      await raiseNotification(notificationType, notificationMessage);

      revalidatePath(`/site/${site}/users`);
      redirect(`/site/${site}/users`);
    });

export const applyAvailabilityTemplate = async (
  request: ApplyAvailabilityTemplateRequest,
): Promise<ServerActionResult<void>> => {
  return appointmentsApi
    .post(`availability/apply-template`, JSON.stringify(request))
    .then(handleEmptyResponse)
    .then(async _ => {
      revalidateTag('availability-created');

      const notificationType = 'ams-notification';
      const notificationMessage =
        'You have successfully created availability for the current site.';
      await raiseNotification(notificationType, notificationMessage);

      revalidateTag(`fetchAvailability`);
      return { success: true, data: undefined };
    });
};

export const saveAvailability = async (
  request: SetAvailabilityRequest,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(`availability`, JSON.stringify(request))
    .then(handleEmptyResponse)
    .then(async _ => {
      revalidateTag('availability-created');

      const notificationType = 'ams-notification';
      const notificationMessage =
        'You have successfully created availability for the current site.';
      await raiseNotification(notificationType, notificationMessage);

      revalidateTag(`fetchAvailability`);
      return { success: true, data: undefined };
    });

export const fetchInformationForCitizens = async (
  site: string,
): Promise<ServerActionResult<string>> => {
  return appointmentsApi
    .get<Site>(`sites/${site}`)
    .then(response =>
      handleBodyResponse(response, data => data.informationForCitizens),
    );
};

export const setSiteInformationForCitizen = async (
  site: string,
  informationForCitizens: SetInformationForCitizensRequest,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(
      `sites/${site}/informationForCitizens`,
      JSON.stringify(informationForCitizens),
    )
    .then(handleEmptyResponse)
    .then(async _ => {
      const notificationType = 'ams-notification';
      const notificationMessage =
        "You have successfully updated the current site's information.";
      await raiseNotification(notificationType, notificationMessage);

      revalidatePath(`/site/${site}/details`);
      return { success: true, data: undefined };
    });

export const fetchBookings = async (
  payload: FetchBookingsRequest,
  statuses: BookingStatus[],
): Promise<ServerActionResult<Booking[]>> => {
  return appointmentsApi
    .post<Booking[]>('booking/query', JSON.stringify(payload))
    .then(response => {
      return handleBodyResponse(response, bookings =>
        bookings.filter(b => statuses.includes(b.status)),
      );
    });
};

export const fetchDailyAvailability = async (
  site: string,
  from: string,
  until: string,
): Promise<ServerActionResult<DailyAvailability[]>> =>
  appointmentsApi
    .get<
      DailyAvailability[]
    >(`daily-availability?site=${site}&from=${from}&until=${until}`)
    .then(handleBodyResponse);

export const fetchWeekSummaryV2 = async (
  site: string,
  from: string,
): Promise<ServerActionResult<WeekSummaryV2>> =>
  appointmentsApi
    .get<WeekSummaryV2>(`week-summary?site=${site}&from=${from}`)
    .then(handleBodyResponse);

export const fetchDaySummary = async (
  site: string,
  from: string,
): Promise<ServerActionResult<DaySummaryV2>> =>
  appointmentsApi
    .get<WeekSummaryV2>(`day-summary?site=${site}&from=${from}`)
    .then(response =>
      handleBodyResponse(response, data => data.daySummaries[0]),
    );

export const fetchBooking = async (
  reference: string,
  site: string,
): Promise<ServerActionResult<Booking>> =>
  appointmentsApi
    .get<Booking>(`booking/${reference}?site=${site}`)
    .then(handleBodyResponse);

export const cancelAppointment = async (
  reference: string,
  site: string,
  cancellationReason: string,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(
      `booking/${reference}/cancel?site=${site}`,
      JSON.stringify({
        cancellationReason,
      }),
    )
    .then(handleEmptyResponse);

export const saveSiteDetails = async (
  site: string,
  details: SetSiteDetailsRequest,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(`sites/${site}/details`, JSON.stringify(details))
    .then(handleEmptyResponse)
    .then(async _ => {
      const notificationType = 'ams-notification';
      const notificationMessage =
        'You have successfully updated the details for the current site.';
      await raiseNotification(notificationType, notificationMessage);

      return { success: true, data: undefined };
    });

export const saveSiteReferenceDetails = async (
  site: string,
  referenceDetails: SetSiteReferenceDetailsRequest,
): Promise<ServerActionResult<void>> => {
  return appointmentsApi
    .post(`sites/${site}/reference-details`, JSON.stringify(referenceDetails))
    .then(handleEmptyResponse)
    .then(async _ => {
      const notificationType = 'ams-notification';
      const notificationMessage =
        'You have successfully updated the reference details for the current site.';
      raiseNotification(notificationType, notificationMessage);
      return { success: true, data: undefined };
    });
};

export const editSession = async (
  request: EditSessionRequest,
): Promise<ServerActionResult<void>> =>
  appointmentsApi
    .post(`availability`, JSON.stringify(request))
    .then(handleEmptyResponse)
    .then(async _ => {
      revalidateTag('availability-created');

      const notificationType = 'ams-notification';
      const notificationMessage = 'You have successfully edited the session.';
      await raiseNotification(notificationType, notificationMessage);

      revalidateTag(`fetchAvailability`);
      return { success: true, data: undefined };
    });

export const cancelSession = async (
  sessionSummary: SessionSummary,
  site: string,
): Promise<ServerActionResult<void>> => {
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
    date: ukStartDatetime.format(RFC3339Format),
    from: ukStartDatetime.format('HH:mm'),
    until: ukEndDatetime.format('HH:mm'),
    services: Object.keys(sessionSummary.totalSupportedAppointmentsByService),
    capacity: sessionSummary.capacity,
    slotLength: sessionSummary.slotLength,
  };

  return appointmentsApi
    .post('session/cancel', JSON.stringify(payload))
    .then(handleEmptyResponse);
};

export const updateSiteStatus = async (
  site: string,
  status: SiteStatus,
): Promise<ServerActionResult<void>> => {
  const payload: UpdateSiteStatusRequest = {
    site,
    status,
  };

  return appointmentsApi
    .post('site-status', JSON.stringify(payload))
    .then(handleEmptyResponse)
    .then(async response => {
      const notificationType = 'ams-notification';
      const notificationMessage =
        status === 'Online'
          ? 'The site is now online and is available for appointments.'
          : 'The site is now offline and will not be available for appointments.';
      await raiseNotification(notificationType, notificationMessage);
      return response;
    })
    .then(response => {
      revalidatePath(`/site/${site}/details`);
      return response;
    });
};

export const cancelDay = async (
  payload: CancelDayRequest,
): Promise<ServerActionResult<CancelDayResponse>> =>
  appointmentsApi
    .post<CancelDayResponse>('day/cancel', JSON.stringify(payload))
    .then(handleBodyResponse);

export const downloadSiteSummaryReport = async (
  startDate: string,
  endDate: string,
): Promise<ServerActionResult<Blob>> =>
  appointmentsApi
    .get<Blob>(`report/site-summary?startDate=${startDate}&endDate=${endDate}`)
    .then(handleBodyResponse);

export const availabilityChangeProposal = async (
  payload: AvailabilityChangeProposalRequest,
): Promise<ServerActionResult<AvailabilityChangeProposalResponse>> =>
  appointmentsApi
    .post<AvailabilityChangeProposalResponse>(
      'availability/propose-edit',
      JSON.stringify(payload),
    )
    .then(handleBodyResponse);
