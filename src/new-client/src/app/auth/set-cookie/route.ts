'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';
import { cookies } from 'next/headers';
import { fetchAccessToken } from '@services/nbsService';
import { revalidateTag } from 'next/cache';

export async function GET(request: NextRequest) {
  const code = request.nextUrl.searchParams.get('code');

  if (code === null) {
    throw Error('No code found in request');
  }

  const token = await fetchAccessToken(code);
  if (token === null) {
    throw Error('No token found in response');
  }

  cookies().set('token', token);
  revalidateTag('user');

  const previousPage = cookies().get('previousPage');
  if (previousPage) {
    cookies().delete('previousPage');
    redirect(previousPage.value);
  }

  redirect('/');
}
