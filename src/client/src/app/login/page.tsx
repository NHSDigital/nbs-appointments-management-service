import { Metadata } from 'next';
import LogInButton from './log-in-button';
import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { fetchFeatureFlag } from '@services/appointmentsService';
import LogInLink from './log-in-link';
import fromServer from '@server/fromServer';

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
  const oktaEnabledFlag = await fromServer(fetchFeatureFlag('OktaEnabled'));
  const { redirectUrl } = { redirectUrl: '/', ...(await searchParams) };

  return (
    <NhsAnonymousPage title="Manage your appointments" originPage="login">
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
        <>
          <br />
          <LogInLink
            redirectUrl={redirectUrl}
            provider={'okta'}
            friendlyName={'Other Email'}
          />
        </>
      )}
    </NhsAnonymousPage>
  );
};

export default Page;
