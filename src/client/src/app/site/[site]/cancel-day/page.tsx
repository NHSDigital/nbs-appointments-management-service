import { notFound } from 'next/navigation';
import {
  fetchSite,
  fetchDaySummary,
  fetchClinicalServices,
  assertPermission,
  assertFeatureEnabled,
} from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';
import CancelDayWizard from './cancel-day-wizard';

type PageProps = {
  searchParams?: Promise<{
    date: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date } = { ...(await searchParams) };

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  await fromServer(assertFeatureEnabled('CancelDay'));

  if (!date) return notFound();

  const [daySummary, site, clinicalServices] = await Promise.all([
    fromServer(fetchDaySummary(siteFromPath, date)),
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  return (
    <NhsTransactionalPage title="" originPage="cancel-day">
      <CancelDayWizard
        date={date}
        site={site}
        daySummary={daySummary}
        clinicalServices={clinicalServices}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
