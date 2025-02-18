import { Metadata } from 'next';
import LogInButton from './log-in-button';
import NhsAnonymousPage from '@components/nhs-anonymous-page';

export const metadata: Metadata = {
  title: 'Manage your appointments',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async () => {
  if (!process.env.AUTH_HOST) {
    throw new Error('AUTH_HOST environment variable is not set');
  }

  return (
    <NhsAnonymousPage title="Manage your appointments" originPage="login">
      <p>
        You are currently not signed in. You must sign in to access this
        service.
      </p>
      <LogInButton
        authHost={process.env.AUTH_HOST}
        provider={'nhs-mail'}
        friendlyName={'NHS Mail'}
      />
      <br />
      <LogInButton
        authHost={process.env.AUTH_HOST}
        provider={'okta'}
        friendlyName={'Other Email'}
      />
    </NhsAnonymousPage>
  );
};

export default Page;
