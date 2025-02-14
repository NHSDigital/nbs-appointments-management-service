import { cookies } from 'next/headers';

export const raiseNotification = (
  notificationType: string,
  notificationMessage: string,
  expiryTimeInSeconds?: number,
) => {
  cookies().set(notificationType, notificationMessage, {
    maxAge: expiryTimeInSeconds ?? 15,
  });
};
