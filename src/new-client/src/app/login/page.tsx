import NhsPage from '@components/nhs-page';
import { Metadata } from 'next';
import LogInButton from './log-in-button';

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
    <NhsPage title="Appointment Management Service" omitTitleFromBreadcrumbs>
      <p>
        You are currently not signed in. You must sign in to access this
        service.
      </p>
      <LogInButton redirectUrl={searchParams?.redirectUrl ?? '/'} />
    </NhsPage>
  );
};

export default Page;
