import {
  assertPermission,
  fetchSite,
  fetchFeatureFlag,
  fetchPermissions,
} from '@services/appointmentsService';
import { ViewAvailabilityPage } from './view-availability-page';
import { RFC3339Format, parseToUkDatetime, ukNow } from '@services/timeService';
import fromServer from '@server/fromServer';
import { Button } from '@components/nhsuk-frontend';
import Link from 'next/link';
import { Heading } from 'nhsuk-react-components';

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

  return (
    <>
      <Heading>
        {`View availability for ${searchMonth.format('MMMM YYYY')}`}
      </Heading>

      {canChangeAvailability && (
        <Link href={`/site/${siteFromPath}/change-availability`}>
          <Button type="button" styleType="secondary">
            Change availability
          </Button>
        </Link>
      )}

      <ViewAvailabilityPage site={site} searchMonth={searchMonth} />
    </>
  );
};

export default Page;
