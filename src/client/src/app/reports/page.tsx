import {
  assertFeatureEnabled,
  assertPermission,
} from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import { ReportsPage } from './reports-page';
import fromServer from '@server/fromServer';

const Page = async () => {
  await fromServer(assertFeatureEnabled('SiteSummaryReport'));
  await fromServer(assertPermission('*', 'reports:sitesummary'));

  return (
    <NhsTransactionalPage originPage="reports">
      <ReportsPage />
    </NhsTransactionalPage>
  );
};

export default Page;
