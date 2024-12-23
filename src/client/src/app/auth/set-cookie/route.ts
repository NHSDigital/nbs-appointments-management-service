'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';
import { cookies } from 'next/headers';
import {
  fetchAccessToken,
  fetchUserProfile,
} from '@services/appointmentsService';
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

  // Implicitly checks EULA is accepted, handles if not
  await fetchUserProfile(`${process.env.CLIENT_BASE_PATH}/eula`);

  const previousPage = cookies().get('previousPage');
  if (previousPage) {
    cookies().delete('previousPage');
    redirect(`${process.env.CLIENT_BASE_PATH}/${previousPage.value}`);
  }

  redirect(process.env.CLIENT_BASE_PATH ?? '/');
}
