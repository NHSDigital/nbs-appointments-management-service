'use server';

import { NhsMyaCookieConsent } from '@types';
import { cookies } from 'next/headers';
import { ukNow } from '@services/timeService';
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
      expires: ukNow().add(1, 'year').toDate(),
    },
  );

  const notificationType = 'nhsuk-mya-cookie-consent-updated';
  const notificationMessage = 'Updated';

  // TODO: I'd like to find a way to "pop" notifications. i.e. remove them the first time they're read.
  // Then we can safely leave max age a little longer to account for race conditions, but know we'll only ever read the notification once
  // NextJS doesn't allow you to do this currently, at least not through a server action like this.
  raiseNotification(notificationType, notificationMessage, 1);
};
