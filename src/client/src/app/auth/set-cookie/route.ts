'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';
import { cookies } from 'next/headers';
import { fetchAccessToken } from '@services/appointmentsService';
import { revalidateTag } from 'next/cache';

export async function GET(request: NextRequest) {
  const code = request.nextUrl.searchParams.get('code');

  if (code === null) {
    throw Error('No code found in request');
  }

  const tokenResponse = await fetchAccessToken(code);
  if (tokenResponse === undefined) {
    throw Error('No token found in response');
  }

  cookies().set('token', tokenResponse.token);
  revalidateTag('user');

  const previousPage = cookies().get('previousPage');
  if (previousPage) {
    cookies().delete('previousPage');
    redirect(previousPage.value);
  }

  redirect('/');
}
