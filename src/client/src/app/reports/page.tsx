import {
  assertFeatureEnabled,
  assertAnyPermission,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import { ReportsPage } from './reports-page';
import fromServer from '@server/fromServer';
import { redirect } from 'next/navigation';

const Page = async () => {
  await fromServer(assertFeatureEnabled('SiteSummaryReport'));
  await fromServer(
    assertAnyPermission('*', [
      'reports:sitesummary',
      'reports:siteusers',
      'reports:master-site-list',
    ]),
  );

  const reportsUplift = await fromServer(fetchFeatureFlag('ReportsUplift'));

  if (reportsUplift.enabled) {
    redirect('/reports/select');
  }

  return (
    <NhsTransactionalPage originPage="reports">
      <ReportsPage />
    </NhsTransactionalPage>
  );
};

export default Page;
