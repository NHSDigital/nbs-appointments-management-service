import NhsTransactionalPage from '@components/nhs-transactional-page';
import ReportsTemplateWizard from './reports-template-wizard';
const Page = async () => {
  return (
    <NhsTransactionalPage originPage="reports-wizard">
      <ReportsTemplateWizard />
    </NhsTransactionalPage>
  );
};

export default Page;
