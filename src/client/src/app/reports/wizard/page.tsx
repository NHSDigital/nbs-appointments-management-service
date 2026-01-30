import NhsTransactionalPage from '@components/nhs-transactional-page';
import ReportsTemplateWizard from './reports-template-wizard';
import fromServer from '@server/fromServer';
import { fetchPermissions } from '@services/appointmentsService';

const Page = async () => {
  const permissions = await fromServer(fetchPermissions('*'));
  return (
    <NhsTransactionalPage originPage="reports-wizard">
      <ReportsTemplateWizard userPermissions={permissions} />
    </NhsTransactionalPage>
  );
};

export default Page;
