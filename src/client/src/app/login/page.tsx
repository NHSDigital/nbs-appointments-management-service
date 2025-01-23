import { Metadata } from 'next';
import LogInButton from './log-in-button';
import NhsHeaderLogIn from '@components/nhs-header-log-in';
import NhsAnonymousPage from '@components/nhs-anonymous-page';

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
  const redirectUrl = searchParams?.redirectUrl ?? '/';
  return (
    <NhsAnonymousPage
      title="Manage your appointments"
      headerAuthComponent={NhsHeaderLogIn({ redirectUrl })}
    >
      <p>
        You are currently not signed in. You must sign in to access this
        service.
      </p>
      <LogInButton
        redirectUrl={redirectUrl}
        provider={'nhs-mail'}
        friendlyName={'NHS Mail'}
      />
      <LogInButton
        redirectUrl={redirectUrl}
        provider={'okta'}
        friendlyName={'Other Email'}
      />
    </NhsAnonymousPage>
  );
};

export default Page;
