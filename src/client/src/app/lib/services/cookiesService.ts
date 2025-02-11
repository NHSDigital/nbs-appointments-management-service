'use server';

import { NhsMyaCookieConsent } from '@types';
import { cookies } from 'next/headers';
import { now } from '@services/timeService';
import { LATEST_CONSENT_COOKIE_VERSION } from '@constants';
import { raiseNotification } from './notificationService';

export const getCookieConsent = async (): Promise<
  NhsMyaCookieConsent | undefined
> => {
  const cookieStore = cookies();
  const nhsukCookieConsent = cookieStore.get('nhsuk-mya-cookie-consent');

  return nhsukCookieConsent
    ? JSON.parse(decodeURIComponent(nhsukCookieConsent.value))
    : undefined;
};

export const setCookieConsent = async (consented: boolean) => {
  const cookieStore = cookies();

  const payload: NhsMyaCookieConsent = {
    consented,
    version: LATEST_CONSENT_COOKIE_VERSION,
  };

  cookieStore.set(
    'nhsuk-mya-cookie-consent',
    encodeURIComponent(JSON.stringify(payload)),
    {
      expires: now().add(1, 'year').toDate(),
    },
  );

  const notificationType = 'nhsuk-mya-cookie-consent-updated';
  const notificationMessage = 'Updated';
  raiseNotification(notificationType, notificationMessage);
};
