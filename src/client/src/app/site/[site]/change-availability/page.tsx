import NhsTransactionalPage from '@components/nhs-transactional-page';
import ChangeAvailabilityWizard from './change-availability-wizard';
import {
  assertFeatureEnabled,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import fromServer from '@server/fromServer';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(assertFeatureEnabled('CancelADateRange'));
  const cancelADateRangeWithBookings = await fromServer(
    fetchFeatureFlag('CancelADateRangeWithBookings'),
  );

  return (
    <NhsTransactionalPage originPage="change-availability-wizard">
      <ChangeAvailabilityWizard
        cancelADateRangeWithBookings={cancelADateRangeWithBookings.enabled}
        site={siteFromPath}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
