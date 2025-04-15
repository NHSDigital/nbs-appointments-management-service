import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
} from '@services/appointmentsService';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import { DailyAppointmentsPage } from './daily-appointments-page';
import { FetchBookingsRequest } from '@types';
import { Tab, Tabs } from '@nhsuk-frontend-components';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import {
  addToUkDate,
  parseDateStringToUkDatetime,
} from '@services/timeService';

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
  await assertPermission(params.site, 'availability:query');

  const date = parseDateStringToUkDatetime(searchParams.date);
  const toDate = addToUkDate(date, 1, 'day');

  const fetchBookingsRequest: FetchBookingsRequest = {
    from: date.format('YYYY-MM-DDTHH:mm:ssZ'),
    to: toDate.format('YYYY-MM-DDTHH:mm:ssZ'),
    site: params.site,
  };

  const [site, bookings] = await Promise.all([
    fetchSite(params.site),
    fetchBookings(fetchBookingsRequest),
  ]);

  const scheduledBookings = bookings.filter(
    b => b.status === 'Booked' && b.availabilityStatus !== 'Orphaned',
  );
  const cancelledBookings = bookings.filter(b => b.status === 'Cancelled');
  const orphanedAppointments = bookings.filter(
    b => b.availabilityStatus === 'Orphaned',
  );

  const orphanedMessage =
    orphanedAppointments.length > 0
      ? `${orphanedAppointments.length} booked appointments are affected. You'll need to manually cancel these appointments.`
      : 'There are no booked appointments affected by availability changes.';

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/view-availability/week?date=${searchParams.date}`,
    text: 'Back to week view',
  };

  const canCancelBookings = (await fetchPermissions(site.id)).includes(
    'booking:cancel',
  );

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
            displayAction={canCancelBookings}
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
            displayAction={canCancelBookings}
            message={orphanedMessage}
          />
        </Tab>
      </Tabs>
    </NhsPage>
  );
};

export default Page;
