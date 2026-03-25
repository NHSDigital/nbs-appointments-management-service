import NhsTransactionalPage from '@components/nhs-transactional-page';
import ChangeAvailabilityWizard from './change-availability-wizard';
import {
  assertFeatureEnabled,
  assertPermission,
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
  const cancelADateRangeMaximumDays = parseInt(
    process.env.CANCEL_A_DATE_RANGE_MAXIMUM_DAYS || '90',
  );
  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  await fromServer(assertFeatureEnabled('CancelADateRange'));
  const cancelADateRangeWithBookings = await fromServer(
    fetchFeatureFlag('CancelADateRangeWithBookings'),
  );

  return (
    <NhsTransactionalPage originPage="change-availability-wizard">
      <ChangeAvailabilityWizard
        cancelADateRangeWithBookings={cancelADateRangeWithBookings.enabled}
        site={siteFromPath}
        rangeMaximumDays={cancelADateRangeMaximumDays}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
