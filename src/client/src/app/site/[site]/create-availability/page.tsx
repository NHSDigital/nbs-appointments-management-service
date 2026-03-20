import NhsPage from '@components/nhs-page';
import { CreateAvailabilityPage } from './create-availability-page';
import {
  fetchSite,
  fetchFeatureFlag,
  fetchPermissions,
  assertPermissionInArray,
} from '@services/appointmentsService';
import fromServer from '@server/fromServer';

type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  const [site, cancelADateRange, sitePermissions] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchFeatureFlag('CancelADateRange')),
    fromServer(fetchPermissions(siteFromPath)),
  ]);

  await assertPermissionInArray(sitePermissions, 'availability:setup');

  const canChangeAvailability =
    cancelADateRange.enabled && sitePermissions.includes('availability:setup');

  return (
    <NhsPage
      title="Create availability"
      caption={site.name}
      site={site}
      originPage="create-availability"
    >
      <CreateAvailabilityPage
        site={site}
        canChangeAvailability={canChangeAvailability}
      />
    </NhsPage>
  );
};

export default Page;
