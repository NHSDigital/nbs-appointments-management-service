import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchAvailability,
  fetchSite,
} from '@services/appointmentsService';
import dayjs from 'dayjs';
import { ViewAvailabilityPage } from './view-availability-page';
import { FetchAvailabilityRequest } from '@types';
import {
  getDetailedMonthView,
  getWeeksInMonth,
} from '@services/viewAvailabilityService';

type PageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    date?: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:query');
  const searchMonth = searchParams?.date
    ? dayjs(searchParams?.date, 'YYYY-MM-DD')
    : dayjs();

  const title = `View availability for ${searchMonth.format('MMMM YYYY')}`;

  const weeks = getWeeksInMonth(searchMonth.year(), searchMonth.month());

  const startDate = weeks[0].startDate.format('YYYY-MM-DD');
  const endDate = weeks[weeks.length - 1].endDate.format('YYYY-MM-DD');
  const payload: FetchAvailabilityRequest = {
    sites: [site.id],
    service: '*',
    from: startDate,
    until: endDate,
    queryType: 'Days',
  };
  const availability = await fetchAvailability(payload);
  const detailedMonthView = await getDetailedMonthView(
    availability,
    weeks,
    site.id,
  );

  return (
    <NhsPage title={title} caption={site.name} site={site}>
      <ViewAvailabilityPage
        weeks={detailedMonthView}
        searchMonth={searchMonth}
      />
    </NhsPage>
  );
};

export default Page;
