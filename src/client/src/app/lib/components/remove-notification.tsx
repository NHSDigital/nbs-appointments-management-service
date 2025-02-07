'use server';
import { cookies } from 'next/headers';

export const removeNotification = async () => {
  const cookieStore = await cookies();

  cookieStore.delete('ams-notification');
};
