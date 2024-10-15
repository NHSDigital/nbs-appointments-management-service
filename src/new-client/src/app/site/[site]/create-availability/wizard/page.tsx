import { fetchSite, fetchUserProfile } from '@services/appointmentsService';
import AvailabilityPeriodWizard from './availability-period-wizard';
import NhsTransactionalPage from '@components/nhs-transactional-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const userProfile = await fetchUserProfile();
  if (site === undefined || userProfile === undefined) {
    throw new Error('Site or profile not found');
  }

  return (
    <NhsTransactionalPage userProfile={userProfile}>
      <AvailabilityPeriodWizard site={site} />
    </NhsTransactionalPage>
  );
};

export default Page;
