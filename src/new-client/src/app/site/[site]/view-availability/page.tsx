import NhsPage from '@components/nhs-page';
import { fetchAvailability, fetchSite } from '@services/appointmentsService';
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
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const title = `View availability for ${dayjs().format('MMMM YYYY')}`;

  const weeks = getWeeksInMonth(dayjs().year(), dayjs().month());

  const firstWeek = weeks[0];
  const lastWeek = weeks[weeks.length - 1];
  const startDate = new Date(
    firstWeek.startYear,
    firstWeek.startMonth,
    firstWeek.start,
  );
  const endDate = new Date(lastWeek.endYear, lastWeek.endMonth, lastWeek.end);
  const payload: FetchAvailabilityRequest = {
    sites: [site.id],
    service: '*',
    from: dayjs(startDate).format('YYYY-MM-DD'),
    until: dayjs(endDate).format('YYYY-MM-DD'),
    queryType: 'Days',
  };
  const availability = await fetchAvailability(payload);
  const detailedMonthView = await getDetailedMonthView(
    availability,
    weeks,
    site.id,
  );

  return (
    <NhsPage
      title={title}
      caption={site.name}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
    >
      <ViewAvailabilityPage weeks={detailedMonthView} />
    </NhsPage>
  );
};

export default Page;
