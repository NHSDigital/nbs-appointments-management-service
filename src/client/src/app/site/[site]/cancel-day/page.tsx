import { notFound } from 'next/navigation';
import {
  fetchSite,
  fetchDaySummary,
  fetchClinicalServices,
  assertPermission,
  assertFeatureEnabled,
} from '@services/appointmentsService';
import { parseToUkDatetime } from '@services/timeService';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
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

  const parsedDate = parseToUkDatetime(date);
  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week?date=${date}`,
    text: 'Back',
  };

  return (
    <NhsTransactionalPage
      title={`Cancel ${parsedDate.format('dddd D MMMM')}`}
      caption={site.name}
      originPage="cancel-day"
      backLink={backLink}
    >
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
