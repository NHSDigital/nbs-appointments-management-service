'use server';
import { cookies } from 'next/headers';

export const removeNotification = async () => {
  cookies().delete('ams-notification');
};
