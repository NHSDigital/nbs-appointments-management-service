import NhsTransactionalPage from '@components/nhs-transactional-page';
import { ReportsSelect } from './reports-select';
import fromServer from '@server/fromServer';
import { fetchPermissions } from '@services/appointmentsService';

const Page = async () => {
  const permissions = await fromServer(fetchPermissions('*'));

  return (
    <NhsTransactionalPage originPage="reports-select">
      <ReportsSelect userPermissions={permissions} />
    </NhsTransactionalPage>
  );
};

export default Page;
