import { assertPermission, fetchSite } from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import AvailabilityTemplateWizard from './availability-template-wizard';

type PageProps = {
  searchParams?: Promise<{
    date: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date } = { ...(await searchParams) };

  await assertPermission(siteFromPath, 'availability:setup');
  const site = await fetchSite(siteFromPath);

  await assertPermission(site.id, 'availability:setup');

  return (
    <NhsTransactionalPage originPage="create-availability-wizard">
      <AvailabilityTemplateWizard site={site} date={date} />
    </NhsTransactionalPage>
  );
};

export default Page;
