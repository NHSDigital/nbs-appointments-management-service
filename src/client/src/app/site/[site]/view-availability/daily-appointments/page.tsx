import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import { DailyAppointmentsPage } from './daily-appointments-page';
import dayjs from 'dayjs';
import { FetchBookingsRequest } from '@types';
import { Tabs, TabsChildren } from '@nhsuk-frontend-components';

type PageProps = {
  searchParams: {
    date: string;
    page: number;
    tab?: string;
  };
  params: {
    site: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:query');
  const title = dayjs(searchParams.date).format('dddd D MMMM');
  const fetchBookingsRequest: FetchBookingsRequest = {
    from: dayjs(searchParams.date)
      .hour(0)
      .minute(0)
      .second(0)
      .format('YYYY-MM-DDTHH:mm:ssZ'),
    to: dayjs(searchParams.date)
      .hour(23)
      .minute(59)
      .second(59)
      .format('YYYY-MM-DDTHH:mm:ssZ'),
    site: site.id,
  };
  const bookings = await fetchBookings(fetchBookingsRequest);
  const buildTabContent = (
    bookingStatus: string,
    displayAction: boolean,
    activeTab?: string,
  ) => {
    return (
      <DailyAppointmentsPage
        bookings={bookings.filter(b => b.status === bookingStatus)}
        page={Number(searchParams.page)}
        date={searchParams.date}
        site={site.id}
        displayAction={displayAction}
        activeTab={activeTab}
      />
    );
  };
  const buildTabs = (siteId: string, activeTab?: string): TabsChildren[] => {
    const tabs = [
      {
        isSelected: activeTab === 'Scheduled',
        url: `/site/${siteId}/view-availability/daily-appointments?date=${searchParams.date}&page=1&tab=Scheduled`,
        tabTitle: 'Scheduled',
        content: buildTabContent('Booked', true, activeTab),
      },
      {
        isSelected: activeTab === 'Cancelled',
        url: `/site/${siteId}/view-availability/daily-appointments?date=${searchParams.date}&page=1&tab=Cancelled`,
        tabTitle: 'Cancelled',
        content: buildTabContent('Cancelled', false, activeTab),
      },
    ];

    if (!tabs.some(t => t.isSelected)) {
      tabs[0].isSelected = true;
    }

    return tabs;
  };

  const tabsChildren = buildTabs(site.id, searchParams.tab);

  return (
    <NhsPage
      title={title}
      caption={site.name}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
        {
          name: 'View daily appointments',
          href: `/site/${params.site}/view-availability/daily-appointments?date=${searchParams.date}&page=${searchParams.page}`,
        },
      ]}
    >
      <Tabs>{tabsChildren}</Tabs>
    </NhsPage>
  );
};

export default Page;
