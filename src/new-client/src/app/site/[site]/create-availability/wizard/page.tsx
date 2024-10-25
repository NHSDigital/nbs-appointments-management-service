import { fetchSite, fetchUserProfile } from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import AvailabilityTemplateWizard from './availability-template-wizard';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const userProfile = await fetchUserProfile();

  // TODO: remove these checks after 202 is merged as the checks will become implicit
  if (site === undefined || userProfile === undefined) {
    throw new Error('Site or profile not found');
  }

  return (
    <NhsTransactionalPage userProfile={userProfile}>
      <AvailabilityTemplateWizard site={site} />
    </NhsTransactionalPage>
  );
};

export default Page;
