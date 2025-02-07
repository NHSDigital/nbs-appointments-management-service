import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { endOfUkWeek, startOfUkWeek } from '@services/timeService';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { notFound } from 'next/navigation';

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

  await assertPermission(siteFromPath, 'availability:query');
  const site = await fetchSite(siteFromPath);

  const { date } = { ...(await searchParams) };
  if (date === undefined) {
    notFound();
  }

  const ukWeekStart = startOfUkWeek(date);
  const ukWeekEnd = endOfUkWeek(date);

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability?date=${date}`,
    text: 'Back to month view',
  };

  return (
    <NhsPage
      title={`${ukWeekStart.format('D MMMM')} to ${ukWeekEnd.format('D MMMM')}`}
      site={site}
      backLink={backLink}
      originPage="view-availability-week"
    >
      <ViewWeekAvailabilityPage
        ukWeekStart={ukWeekStart}
        ukWeekEnd={ukWeekEnd}
        site={site}
      />
    </NhsPage>
  );
};

export default Page;
