import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { endOfWeek, startOfWeek } from '@services/timeService';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { summariseWeek } from '@services/availabilityCalculatorService';
import { Suspense } from 'react';
import { Spinner } from '@components/nhsuk-frontend';

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
      <Suspense fallback={<Spinner />}>
        <ViewWeekAvailabilityPage
          days={summariseWeek(weekStart, weekEnd, site.id)}
          weekStart={weekStart}
          weekEnd={weekEnd}
          site={params.site}
        />
      </Suspense>
    </NhsPage>
  );
};

export default Page;
