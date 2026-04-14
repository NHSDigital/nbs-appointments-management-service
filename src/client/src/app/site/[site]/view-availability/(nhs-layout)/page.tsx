import {
  assertPermission,
  fetchSite,
  fetchFeatureFlag,
  fetchPermissions,
} from '@services/appointmentsService';
import { ViewAvailabilityPage } from './view-availability-page';
import { RFC3339Format, parseToUkDatetime, ukNow } from '@services/timeService';
import fromServer from '@server/fromServer';
import Link from 'next/link';
import { Heading, Button } from 'nhsuk-react-components';

type PageProps = {
  searchParams?: Promise<{
    date?: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const { date } = { ...(await searchParams) };
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(assertPermission(siteFromPath, 'availability:query'));

  const [cancelADateRange, site, sitePermissions] = await Promise.all([
    fromServer(fetchFeatureFlag('CancelADateRange')),
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchPermissions(siteFromPath)),
  ]);

  const canChangeAvailability =
    cancelADateRange.enabled && sitePermissions.includes('availability:setup');

  const searchMonth = date ? parseToUkDatetime(date, RFC3339Format) : ukNow();

  // Construct the return URL for the current view
  // We include the date so the back button knows exactly which month to return to
  const currentViewPath = `/site/${siteFromPath}/view-availability${date ? `?date=${date}` : ''}`;

  // Encode it so it's safe to put inside another URL
  const encodedReturnUrl = encodeURIComponent(currentViewPath);

  return (
    <>
      <Heading headingLevel="h2">
        <span className="nhsuk-caption-l">{site.name}</span>
        {`View availability for ${searchMonth.format('MMMM YYYY')}`}
      </Heading>

      {canChangeAvailability && (
        /* Pass the returnUrl to the wizard */
        <Link
          href={`/site/${siteFromPath}/change-availability?returnUrl=${encodedReturnUrl}`}
        >
          <Button type="button" secondary>
            Change availability
          </Button>
        </Link>
      )}

      <ViewAvailabilityPage site={site} searchMonth={searchMonth} />
    </>
  );
};

export default Page;
