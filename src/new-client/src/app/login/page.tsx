import { Metadata } from 'next';
import LogInButton from './log-in-button';
import NhsAnonymousPage from '@components/nhs-anonymous-page';

export type LoginPageProps = {
  searchParams?: {
    redirectUrl?: string;
  };
};

export const metadata: Metadata = {
  title: 'Appointment Management Service',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async ({ searchParams }: LoginPageProps) => {
  return (
    <NhsAnonymousPage title="Appointment Management Service">
      <p>
        You are currently not signed in. You must sign in to access this
        service.
      </p>
      <LogInButton redirectUrl={searchParams?.redirectUrl ?? '/'} />
    </NhsAnonymousPage>
  );
};

export default Page;
