import { fetchUserProfile } from '@services/appointmentsService';
import SiteList from '@components/site-list';
import NhsPage from '@components/nhs-page';

const Home = async () => {
  const userProfile = await fetchUserProfile();

  return (
    <NhsPage title="Appointment Management Service">
      <SiteList sites={userProfile?.availableSites ?? []} />
    </NhsPage>
  );
};

export default Home;
