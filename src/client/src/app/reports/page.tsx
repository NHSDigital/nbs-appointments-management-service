import { fetchSite, assertPermission } from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import { ReportsPage } from './reports-page';

type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await assertPermission(siteFromPath, 'reports:sitesummary');

  const [site] = await Promise.all([fetchSite(siteFromPath)]);

  return (
    <NhsTransactionalPage originPage="reports">
      <ReportsPage site={site} />
    </NhsTransactionalPage>
  );
};

export default Page;
