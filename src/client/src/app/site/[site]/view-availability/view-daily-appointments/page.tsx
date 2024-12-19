import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import { ViewDailyAppointmentsPage } from './view-daily-appointments-page';
import dayjs from 'dayjs';
import { FetchBookingsRequest } from '@types';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

type PageProps = {
  searchParams: {
    date: string;
    page: number;
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
  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/view-availability/week?date=${searchParams.date}`,
    text: 'Back to week view',
  };

  return (
    <NhsPage title={title} caption={site.name} backLink={backLink}>
      <ViewDailyAppointmentsPage
        bookings={bookings.filter(b => b.status === 'Booked')}
        page={Number(searchParams.page)}
        date={searchParams.date}
        site={site.id}
      />
    </NhsPage>
  );
};

export default Page;