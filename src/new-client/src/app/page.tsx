import { fetchUserProfile } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { Metadata } from 'next';
import { HomePage } from './home-page';

export const metadata: Metadata = {
  title: 'Manage your appointments',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async () => {
  const userProfile = await fetchUserProfile();

  return (
    <NhsPage title="Manage your appointments" omitTitleFromBreadcrumbs>
      <HomePage sites={userProfile.availableSites} />
    </NhsPage>
  );
};

export default Page;
