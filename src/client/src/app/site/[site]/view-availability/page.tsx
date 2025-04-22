import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { ViewAvailabilityPage } from './view-availability-page';
import {
  dateStringFormat,
  parseToUkDatetime,
  ukNow,
} from '@services/timeService';

type PageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    date?: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  await assertPermission(params.site, 'availability:query');
  const site = await fetchSite(params.site);

  const searchMonth = searchParams?.date
    ? parseToUkDatetime(searchParams?.date, dateStringFormat)
    : ukNow();

  return (
    <NhsPage
      title={`View availability for ${searchMonth.format('MMMM YYYY')}`}
      caption={site.name}
      site={site}
      originPage="view-availability"
    >
      <ViewAvailabilityPage site={site} searchMonth={searchMonth} />
    </NhsPage>
  );
};

export default Page;
