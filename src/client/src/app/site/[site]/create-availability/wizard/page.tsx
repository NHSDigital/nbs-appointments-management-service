import { fetchSite } from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import AvailabilityTemplateWizard from './availability-template-wizard';

type PageProps = {
  searchParams: {
    date: string;
  };
  params: {
    site: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const site = await fetchSite(params.site);

  return (
    <NhsTransactionalPage originPage="create-availability-wizard">
      <AvailabilityTemplateWizard site={site} date={searchParams.date} />
    </NhsTransactionalPage>
  );
};

export default Page;
