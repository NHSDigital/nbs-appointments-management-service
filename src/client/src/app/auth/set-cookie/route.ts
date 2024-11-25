'use server';
import { redirect } from 'next/navigation';
import { NextRequest } from 'next/server';
import { cookies } from 'next/headers';
import { fetchAccessToken, fetchEula } from '@services/appointmentsService';
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

// TODO: Move this from appointmentsService.ts into here
// const assertEula = async (userProfile: UserProfile) => {
//   const eulaCookie = cookies().get('eula-consent');
//   if (eulaCookie === undefined) {
//     const latestEulaVersion = await fetchEula();
//     console.dir({
//       fromApi: latestEulaVersion,
//       fromProfile: userProfile.latestAcceptedEulaVersion,
//     });
//     if (
//       latestEulaVersion.versionDate === userProfile.latestAcceptedEulaVersion
//     ) {
//       console.log('trying to set cookie');
//       cookies().set('eula-consent', 'true', { maxAge: 5000 });
//     } else {
//       redirect('/eula');
//     }
//   }
// };
