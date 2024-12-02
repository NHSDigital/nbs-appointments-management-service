import { cookies } from 'next/headers';

export const raiseNotification = (
  notificationType: string,
  notificationMessage: string,
) => {
  cookies().set(notificationType, notificationMessage, {
    maxAge: 15, // 15 seconds
  });
};
