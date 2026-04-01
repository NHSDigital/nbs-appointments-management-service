import {
  assertPermission,
  fetchSite,
  fetchFeatureFlag,
  fetchPermissions,
} from '@services/appointmentsService';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { endOfUkWeek, startOfUkWeek } from '@services/timeService';
import { notFound } from 'next/navigation';
import fromServer from '@server/fromServer';
import { Button } from '@components/nhsuk-frontend';
import Link from 'next/link';
import { Heading } from 'nhsuk-react-components';

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

  const [cancelADateRange, site, sitePermissions] = await Promise.all([
    fromServer(fetchFeatureFlag('CancelADateRange')),
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchPermissions(siteFromPath)),
  ]);

  const canChangeAvailability =
    cancelADateRange.enabled && sitePermissions.includes('availability:setup');

  const ukWeekStart = startOfUkWeek(date);
  const ukWeekEnd = endOfUkWeek(date);

  return (
    <>
      <Heading headingLevel="h2">
        <span className="nhsuk-caption-l">{site.name}</span>
        {`${ukWeekStart.format('D MMMM')} to ${ukWeekEnd.format('D MMMM')}`}
      </Heading>

      {canChangeAvailability && (
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
    </>
  );
};

export default Page;
