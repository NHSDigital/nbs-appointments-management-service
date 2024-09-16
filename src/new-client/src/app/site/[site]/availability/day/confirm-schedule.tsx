'use server';
import { revalidatePath } from 'next/cache';
import { redirect } from 'next/navigation';
import { cookies } from 'next/headers';

export const confirmSchedule = async (site: string, date: string) => {
  cookies().set(
    'ams-notification',
    `You have successfully saved the schedule for ${date}.`,
    {
      maxAge: 15, // 15 seconds
    },
  );

  revalidatePath(`/site/${site}/availability/month?date=${date}`);
  redirect(`/site/${site}/availability/month?date=${date}`);
};
