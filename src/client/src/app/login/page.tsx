import { Metadata } from 'next';
import LogInButton from './log-in-button';
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
      {/* TODO add feature toggle management to UI */}
      <LogInButton
        redirectUrl={redirectUrl}
        provider={'okta'}
        friendlyName={'Other Email'}
      />
    </NhsAnonymousPage>
  );
};

export default Page;
