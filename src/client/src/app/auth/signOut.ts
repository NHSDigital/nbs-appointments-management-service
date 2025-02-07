'use server';
import { revalidatePath } from 'next/cache';
import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';

export const signOut = async () => {
  const cookieStore = await cookies();

  cookieStore.delete('token');
  revalidatePath('/login');
  redirect('/login');
};
