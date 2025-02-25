'use server';
import { revalidatePath } from 'next/cache';
import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';

export const signOut = async () => {
  cookies().delete('token');
  revalidatePath('/login');
  redirect('/login');
};
