import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchDailyAvailability,
  fetchSite,
} from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { endOfWeek, startOfWeek } from '@services/timeService';
import { getDetailedWeekView } from '@services/viewAvailabilityService';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

type PageProps = {
  searchParams: {
    date: string;
  };
  params: {
    site: string;
  };
};

const Page = async ({ searchParams, params }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:query');

  const weekStart = startOfWeek(searchParams.date);
  const weekEnd = endOfWeek(searchParams.date);

  const availability = await fetchDailyAvailability(
    params.site,
    weekStart.format('YYYY-MM-DD'),
    weekEnd.format('YYYY-MM-DD'),
  );

  const days = await getDetailedWeekView(
    weekStart,
    weekEnd,
    site.id,
    availability,
  );

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/view-availability?date=${searchParams.date}`,
    text: 'Back to month view',
  };

  return (
    <NhsPage
      title={`${weekStart.format('D MMMM')} to ${weekEnd.format('D MMMM')}`}
      site={site}
      backLink={backLink}
    >
      <ViewWeekAvailabilityPage
        days={days}
        weekStart={weekStart}
        weekEnd={weekEnd}
      />
    </NhsPage>
  );
};

export default Page;
