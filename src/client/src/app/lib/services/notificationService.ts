import { cookies } from 'next/headers';

export const raiseNotification = async (
  notificationType: string,
  notificationMessage: string,
  notificationPath?: string,
  expiryTimeInSeconds?: number,
) => {
  const cookieStore = await cookies();
  const path =
    notificationPath === undefined
      ? '/'
      : `/manage-your-appointments/${notificationPath}`;

  cookieStore.set(notificationType, notificationMessage, {
    maxAge: expiryTimeInSeconds ?? 15,
    path: path,
  });
};
