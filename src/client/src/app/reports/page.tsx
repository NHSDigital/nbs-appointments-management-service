import {
  assertFeatureEnabled,
  assertPermission,
} from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import { ReportsPage } from './reports-page';

const Page = async () => {
  await assertFeatureEnabled('SiteSummaryReport');
  await assertPermission('*', 'reports:sitesummary');

  return (
    <NhsTransactionalPage originPage="reports">
      <ReportsPage />
    </NhsTransactionalPage>
  );
};

export default Page;
