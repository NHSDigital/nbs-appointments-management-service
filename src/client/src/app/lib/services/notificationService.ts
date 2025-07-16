import { cookies } from 'next/headers';

export const raiseNotification = async (
  notificationType: string,
  notificationMessage: string,
  expiryTimeInSeconds?: number,
) => {
  const cookieStore = await cookies();

  cookieStore.set(notificationType, notificationMessage, {
    maxAge: expiryTimeInSeconds ?? 15,
  });
};
