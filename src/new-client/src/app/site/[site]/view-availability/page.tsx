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
import Pagination from '@components/nhsuk-frontend/pagination';
import customParseFormat from 'dayjs/plugin/customParseFormat';

dayjs.extend(customParseFormat);

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

  const nextMonth = searchMonth.startOf('month').add(1, 'month');
  const previousMonth = searchMonth.startOf('month').subtract(1, 'month');

  const next = {
    title: nextMonth.format('MMMM YYYY'),
    href: `view-availability?date=${nextMonth.format('YYYY-MM-DD')}`,
  };
  const previous = {
    title: previousMonth.format('MMMM YYYY'),
    href: `view-availability?date=${previousMonth.format('YYYY-MM-DD')}`,
  };

  return (
    <NhsPage
      title={title}
      caption={site.name}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
    >
      <Pagination previous={previous} next={next} />
      <ViewAvailabilityPage weeks={detailedMonthView} />
    </NhsPage>
  );
};

export default Page;
