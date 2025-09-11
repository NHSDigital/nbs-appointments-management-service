'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';
import { cookies } from 'next/headers';
import {
  fetchAccessToken,
  fetchUserProfile,
} from '@services/appointmentsService';
import { revalidateTag } from 'next/cache';
import fromServer from '@server/fromServer';

export async function GET(request: NextRequest) {
  const code = request.nextUrl.searchParams.get('code');
  const provider = request.nextUrl.searchParams.get('provider');
  const cookieStore = await cookies();

  if (code === null || code === '') {
    throw Error('No code found in request');
  }

  if (provider === null || provider === '') {
    throw Error('No provider found in request');
  }

  const token = await fromServer(fetchAccessToken(code, provider));

  cookieStore.set('token', token);
  revalidateTag('user');

  // Implicitly checks EULA is accepted, handles if not
  await fromServer(fetchUserProfile(`${process.env.CLIENT_BASE_PATH}/eula`));

  const previousPage = cookieStore.get('previousPage');
  if (previousPage) {
    cookieStore.delete('previousPage');
    redirect(`${process.env.CLIENT_BASE_PATH}/${previousPage.value}`);
  }

  redirect(`${process.env.CLIENT_BASE_PATH}/sites`);
}
