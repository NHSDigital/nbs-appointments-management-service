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

  let site = undefined;
  await assertPermission(siteFromPath ?? '*', 'reports:sitesummary');
  if (siteFromPath !== undefined) {
    site = await fetchSite(siteFromPath);
  }
  return (
    <NhsTransactionalPage originPage="reports">
      <ReportsPage site={site ?? undefined} />
    </NhsTransactionalPage>
  );
};

export default Page;
