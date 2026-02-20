import NhsTransactionalPage from '@components/nhs-transactional-page';
import ChangeAvailabilityWizard from './change-availability-wizard';

const Page = async () => {
  return (
    <NhsTransactionalPage originPage="change-availability-wizard">
      <ChangeAvailabilityWizard />
    </NhsTransactionalPage>
  );
};

export default Page;
