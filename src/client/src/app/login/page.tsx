import { Metadata } from 'next';
import LogInButton from './log-in-button';
import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { fetchFeatureFlag } from '@services/appointmentsService';

export type LoginPageProps = {
  searchParams?: {
    redirectUrl?: string;
  };
};

export const metadata: Metadata = {
  title: 'Manage your appointments',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async ({ searchParams }: LoginPageProps) => {
  const oktaLoginFlag = await fetchFeatureFlag('OktaLogin');
  const redirectUrl = searchParams?.redirectUrl ?? '/sites';
  return (
    <NhsAnonymousPage title="Manage your appointments" originPage="login">
      <p>
        You are currently not signed in. You must sign in to access this
        service.
      </p>
      <p style={{ display: 'none' }}>
        Auth: {process.env.AUTH_HOST} Base Url: {process.env.NBS_API_BASE_URL}
      </p>
      <LogInButton
        redirectUrl={redirectUrl}
        provider={'nhs-mail'}
        friendlyName={'NHS Mail'}
      />
      {oktaLoginFlag.enabled && (
        <LogInButton
          redirectUrl={redirectUrl}
          provider={'okta'}
          friendlyName={'Other Email'}
        />
      )}
    </NhsAnonymousPage>
  );
};

export default Page;
