import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { endOfWeek, startOfWeek } from '@services/timeService';
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
  await assertPermission(params.site, 'availability:query');
  const site = await fetchSite(params.site);

  const weekStart = startOfWeek(searchParams.date);
  const weekEnd = endOfWeek(searchParams.date);

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
      originPage="view-availability-week"
    >
      <ViewWeekAvailabilityPage
        weekStart={weekStart}
        weekEnd={weekEnd}
        site={site}
      />
    </NhsPage>
  );
};

export default Page;
