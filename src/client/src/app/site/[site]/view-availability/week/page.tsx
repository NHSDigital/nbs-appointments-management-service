import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { ukEndOfWeek, ukStartOfWeek } from '@services/timeService';
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

  const ukWeekStart = ukStartOfWeek(searchParams.date);
  const ukWeekEnd = ukEndOfWeek(searchParams.date);

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/view-availability?date=${searchParams.date}`,
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
