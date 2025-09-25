import {
  assertPermission,
  fetchSite,
  fetchDaySummary,
  fetchClinicalServices,
} from '@services/appointmentsService';
import CancelAppointmentsPage from './cancel-appointments';
import { notFound } from 'next/navigation';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import { parseToUkDatetime } from '@services/timeService';
import fromServer from '@server/fromServer';

type PageProps = {
  params: Promise<{
    site: string;
    date: string;
    bookingsCount: string; // passed as string from URL
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath, date, bookingsCount } = await params;

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const parsedDate = parseToUkDatetime(date);
  const [site, clinicalServices, daySummary] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
    fromServer(fetchDaySummary(siteFromPath, date)),
  ]);

  const affectedCount = parseInt(bookingsCount, 10);
  if (isNaN(affectedCount) || affectedCount <= 0) {
    notFound();
  }

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/daily-appointments?date=${date}&page=1`,
    text: 'Go back',
  };

  return (
    <NhsTransactionalPage
      caption="Cancel appointments"
      title={`New time and capacity for ${parsedDate.format('DD MMMM YYYY')}`}
      originPage="bulk-appointment-cancel"
      backLink={backLink}
    >
      <CancelAppointmentsPage
        site={site.name}
        date={date}
        daySummary={daySummary}
        affectedCount={affectedCount}
        clinicalServices={clinicalServices}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
