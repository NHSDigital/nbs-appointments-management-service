import { fetchSite } from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import AvailabilityTemplateWizard from './availability-template-wizard';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);

  return (
    <NhsTransactionalPage>
      <AvailabilityTemplateWizard site={site} />
    </NhsTransactionalPage>
  );
};

export default Page;
