'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';
import { cookies } from 'next/headers';
import {
  assertEulaAcceptance,
  fetchAccessToken,
  fetchUserProfile,
} from '@services/appointmentsService';
import { revalidateTag } from 'next/cache';

export async function GET(request: NextRequest) {
  const code = request.nextUrl.searchParams.get('code');
  const provider = request.nextUrl.searchParams.get('provider');

  if (code === null) {
    throw Error('No code found in request');
  }

  if (provider === null) {
    throw Error('No provider found in request');
  }

  const tokenResponse = await fetchAccessToken(code, provider);
  if (tokenResponse === undefined) {
    throw Error('No token found in response');
  }

  cookies().set('token', tokenResponse.token);
  revalidateTag('user');

  const userProfile = await fetchUserProfile();
  await assertEulaAcceptance(userProfile);

  const previousPage = cookies().get('previousPage');
  if (previousPage) {
    cookies().delete('previousPage');
    redirect(previousPage.value);
  }

  redirect('/');
}
