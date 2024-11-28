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
  monthEnd,
  monthStart,
} from '@services/viewAvailabilityService';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:query');
  const title = `View availability for ${dayjs().format('MMMM YYYY')}`;

  const weeks = getWeeksInMonth(dayjs().year(), dayjs().month());

  const startDate = monthStart(weeks);
  const endDate = monthEnd(weeks);
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
