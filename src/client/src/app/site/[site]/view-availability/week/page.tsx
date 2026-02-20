import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { endOfUkWeek, startOfUkWeek } from '@services/timeService';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { notFound } from 'next/navigation';
import fromServer from '@server/fromServer';
import { Button } from '@components/nhsuk-frontend';
import Link from 'next/link';

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
  if (date === undefined) {
    notFound();
  }

  await fromServer(assertPermission(siteFromPath, 'availability:query'));

  const [cancelADateRangeFeature, site] = await Promise.all([
    fromServer(fetchFeatureFlag('CancelADateRange')),
    fromServer(fetchSite(siteFromPath)),
  ]);

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
      {cancelADateRangeFeature.enabled == true && (
        <Link href={`/site/${siteFromPath}/change-availability`}>
          <Button type="button" styleType="secondary">
            Change availability
          </Button>
        </Link>
      )}

      <ViewWeekAvailabilityPage
        ukWeekStart={ukWeekStart}
        ukWeekEnd={ukWeekEnd}
        site={site}
      />
    </NhsPage>
  );
};

export default Page;
