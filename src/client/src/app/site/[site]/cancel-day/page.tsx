import NhsPage from '@components/nhs-page';
import { notFound } from 'next/navigation';
import {
  fetchSite,
  fetchDaySummary,
  fetchClinicalServices,
  assertPermission,
  assertFeatureEnabled,
} from '@services/appointmentsService';
import { parseToUkDatetime } from '@services/timeService';
import CancelDayForm from './cancel-day-form';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

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

  await assertPermission(siteFromPath, 'availability:setup');
  await assertFeatureEnabled('CancelDay');

  if (!date) return notFound();

  const [daySummary, site, clinicalServices] = await Promise.all([
    fetchDaySummary(siteFromPath, date),
    fetchSite(siteFromPath),
    fetchClinicalServices(),
  ]);

  const parsedDate = parseToUkDatetime(date);
  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week?date=${date}`,
    text: 'Back',
  };

  return (
    <NhsPage
      title={`Cancel ${parsedDate.format('dddd D MMMM')}`}
      caption={site.name}
      originPage="cancel-day"
      backLink={backLink}
    >
      <CancelDayForm
        date={date}
        siteId={site.id}
        daySummary={daySummary}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
