import NhsPage from '@components/nhs-page';
import { CreateAvailabilityPage } from './create-availability-page';
import {
  assertPermission,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import fromServer from '@server/fromServer';

type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  const [site, cancelADateRange] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchFeatureFlag('CancelADateRange')),
  ]);

  return (
    <NhsPage
      title="Create availability"
      caption={site.name}
      site={site}
      originPage="create-availability"
    >
      <CreateAvailabilityPage
        site={site}
        cancelADateRange={cancelADateRange.enabled}
      />
    </NhsPage>
  );
};

export default Page;
