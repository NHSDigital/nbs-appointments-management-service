import { Metadata } from 'next';
import LogInButton from './log-in-button';
import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { fetchFeatureFlag } from '@services/appointmentsService';
import LogInLink from './log-in-link';
import { WarningCallout } from '@components/nhsuk-frontend';

export type LoginPageProps = {
  searchParams?: Promise<{
    redirectUrl?: string;
  }>;
};

export const metadata: Metadata = {
  title: 'Manage your appointments',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async ({ searchParams }: LoginPageProps) => {
  const oktaEnabledFlag = await fetchFeatureFlag('OktaEnabled');
  const { redirectUrl } = { redirectUrl: '/', ...(await searchParams) };

  return (
    <NhsAnonymousPage title="Manage your appointments" originPage="login">
      <WarningCallout title="There is a known issue with signing in">
        When signing in, you may be shown an error. If this happens, refresh the
        page and you will see the sign-in screen.
      </WarningCallout>
      <p>
        You are currently not signed in. You must sign in to access this
        service.
      </p>
      <LogInButton
        redirectUrl={redirectUrl}
        provider={'nhs-mail'}
        friendlyName={'NHS Mail'}
      />
      {oktaEnabledFlag.enabled && (
        <LogInLink
          redirectUrl={redirectUrl}
          provider={'okta'}
          friendlyName={'Other Email'}
        />
      )}
    </NhsAnonymousPage>
  );
};

export default Page;
