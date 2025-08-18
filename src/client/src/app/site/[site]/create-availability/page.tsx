import NhsPage from '@components/nhs-page';
import { CreateAvailabilityPage } from './create-availability-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import fromServer from '@server/fromServer';

type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  const site = await fromServer(fetchSite(siteFromPath));

  return (
    <NhsPage
      title="Create availability"
      caption={site.name}
      site={site}
      originPage="create-availability"
    >
      <CreateAvailabilityPage site={site} />
    </NhsPage>
  );
};

export default Page;
