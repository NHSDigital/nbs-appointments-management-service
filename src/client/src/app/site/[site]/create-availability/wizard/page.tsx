import { assertPermission, fetchSite } from '@services/appointmentsService';
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
  await assertPermission(params.site, 'availability:setup');
  const site = await fetchSite(params.site);

  await assertPermission(site.id, 'availability:setup');

  return (
    <NhsTransactionalPage originPage="create-availability-wizard">
      <AvailabilityTemplateWizard site={site} date={searchParams.date} />
    </NhsTransactionalPage>
  );
};

export default Page;
