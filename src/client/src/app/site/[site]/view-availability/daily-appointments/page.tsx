import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import { DailyAppointmentsPage } from './daily-appointments-page';
import dayjs from 'dayjs';
import { FetchBookingsRequest } from '@types';
import { Tab, Tabs } from '@nhsuk-frontend-components';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

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
  const date = dayjs(searchParams.date);

  const fetchBookingsRequest: FetchBookingsRequest = {
    from: date.hour(0).minute(0).second(0).format('YYYY-MM-DDTHH:mm:ssZ'),
    to: date.hour(23).minute(59).second(59).format('YYYY-MM-DDTHH:mm:ssZ'),
    site: site.id,
  };

  const bookings = await fetchBookings(fetchBookingsRequest);
  const scheduledBookings = bookings.filter(b => b.status === 'Booked');
  const cancelledBookings = bookings.filter(b => b.status === 'Cancelled');
  const orphanedAppointments = bookings.filter(b => b.status === 'Orphaned');

  const orphanedMessage =
    orphanedAppointments.length > 0
      ? `${orphanedAppointments.length} booked appointments are affected. You'll need to manually cancel these appointments.`
      : 'There are no booked appointments affected by availability changes.';

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/view-availability/week?date=${searchParams.date}`,
    text: 'Back to week view',
  };

  return (
    <NhsPage
      title={date.format('dddd D MMMM')}
      caption={site.name}
      backLink={backLink}
      originPage="view-availability-daily-appointments"
    >
      <Tabs paramsToSetOnTabChange={[{ key: 'page', value: '1' }]}>
        <Tab title="Scheduled">
          <DailyAppointmentsPage
            bookings={scheduledBookings}
            site={site.id}
            displayAction
          />
        </Tab>
        <Tab title="Cancelled">
          <DailyAppointmentsPage
            bookings={cancelledBookings}
            site={site.id}
            displayAction={false}
          />
        </Tab>
        <Tab title="Manual Cancellations">
          <DailyAppointmentsPage
            bookings={orphanedAppointments}
            site={site.id}
            displayAction={false}
            message={orphanedMessage}
          />
        </Tab>
      </Tabs>
    </NhsPage>
  );
};

export default Page;
