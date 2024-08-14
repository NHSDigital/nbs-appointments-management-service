import { fetchUserProfile } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { Metadata } from 'next';
import SiteList from '@components/site-list';
import { Site } from '@types';

export const metadata: Metadata = {
  title: 'Appointment Management Service',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async () => {
  const userProfile = await fetchUserProfile();

  return (
    <NhsPage title="Appointment Management Service">
      <HomePage sites={userProfile?.availableSites ?? []} />
    </NhsPage>
  );
};

interface HomePageProps {
  sites: Site[];
}

export const HomePage = ({ sites }: HomePageProps) => {
  return <SiteList sites={sites} />;
};

export default Page;
