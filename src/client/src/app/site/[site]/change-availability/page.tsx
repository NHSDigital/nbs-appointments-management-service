import NhsTransactionalPage from '@components/nhs-transactional-page';
import ChangeAvailabilityWizard from './change-availability-wizard';
import { assertFeatureEnabled } from '@services/appointmentsService';
import fromServer from '@server/fromServer';

const Page = async () => {
  await fromServer(assertFeatureEnabled('CancelADateRange'));

  return (
    <NhsTransactionalPage originPage="change-availability-wizard">
      <ChangeAvailabilityWizard />
    </NhsTransactionalPage>
  );
};

export default Page;
