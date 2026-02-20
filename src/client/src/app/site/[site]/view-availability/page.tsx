import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { ViewAvailabilityPage } from './view-availability-page';
import { RFC3339Format, parseToUkDatetime, ukNow } from '@services/timeService';
import fromServer from '@server/fromServer';
import { Button } from '@components/nhsuk-frontend';
import Link from 'next/link';

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

  const [cancelADateRangeFeature, site] = await Promise.all([
    fromServer(fetchFeatureFlag('CancelADateRange')),
    fromServer(fetchSite(siteFromPath)),
  ]);

  const searchMonth = date ? parseToUkDatetime(date, RFC3339Format) : ukNow();

  return (
    <NhsPage
      title={`View availability for ${searchMonth.format('MMMM YYYY')}`}
      caption={site.name}
      site={site}
      originPage="view-availability"
    >
      {cancelADateRangeFeature.enabled && (
        <Link href={`/site/${siteFromPath}/change-availability`}>
          <Button type="button" styleType="secondary">
            Change availability
          </Button>
        </Link>
      )}

      <ViewAvailabilityPage site={site} searchMonth={searchMonth} />
    </NhsPage>
  );
};

export default Page;
