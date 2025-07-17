import { fetchSite, assertPermission } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';

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
    <NhsPage title="Reports" site={site} originPage="reports">
      <div>This will be the reports page</div>
    </NhsPage>
  );
};

export default Page;
