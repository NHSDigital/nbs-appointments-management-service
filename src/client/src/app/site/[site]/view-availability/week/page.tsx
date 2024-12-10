import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchDailyAvailability,
  fetchSite,
} from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { endOfWeek, startOfWeek } from '@services/timeService';

// TODO: Include site in props
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

  return (
    <NhsPage
      title={`${weekStart.format('D MMMM')} to ${weekEnd.format('D MMMM')}`}
      // TODO: Does the view availability breadcrumb need a date query param? Or date in the name?
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
        {
          name: 'View availability',
          href: `/site/${params.site}/view-availability`,
        },
      ]}
      site={site}
    >
      <ViewWeekAvailabilityPage availability={availability} />
    </NhsPage>
  );
};

export default Page;
