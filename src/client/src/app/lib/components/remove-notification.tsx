'use server';
import { cookies } from 'next/headers';

const removeNotification = async () => {
  cookies().delete('ams-notification');
};

export default removeNotification;
