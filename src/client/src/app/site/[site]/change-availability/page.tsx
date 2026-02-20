import NhsTransactionalPage from '@components/nhs-transactional-page';
import ChangeAvailabilityWizard from './change-availability-wizard';
import {
  assertFeatureEnabled,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import fromServer from '@server/fromServer';

const Page = async () => {
  await fromServer(assertFeatureEnabled('CancelADateRange'));
  const cancelADateRangeWithBookings = await fromServer(
    fetchFeatureFlag('CancelADateRangeWithBookings'),
  );

  return (
    <NhsTransactionalPage originPage="change-availability-wizard">
      <ChangeAvailabilityWizard
        cancelADateRangeWithBookings={cancelADateRangeWithBookings.enabled}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
