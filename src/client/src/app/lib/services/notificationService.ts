import { cookies } from 'next/headers';

export const raiseNotification = async (
  notificationType: string,
  notificationMessage: string,
) => {
  const cookieStore = await cookies();

  cookieStore.set(notificationType, notificationMessage, {
    maxAge: 15, // 15 seconds
  });
};
