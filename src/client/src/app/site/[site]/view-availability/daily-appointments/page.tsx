import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
} from '@services/appointmentsService';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import { DailyAppointmentsPage } from './daily-appointments-page';
import dayjs from 'dayjs';
import { FetchBookingsRequest } from '@types';
import { Tab, Tabs } from '@nhsuk-frontend-components';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { notFound } from 'next/navigation';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    page: number;
    tab?: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date, page } = { ...(await searchParams) };
  if (date === undefined || page === undefined) {
    notFound();
  }

  await assertPermission(siteFromPath, 'availability:query');

  const parsedDate = dayjs(date);
  const fetchBookingsRequest: FetchBookingsRequest = {
    from: parsedDate.hour(0).minute(0).second(0).format('YYYY-MM-DDTHH:mm:ssZ'),
    to: parsedDate
      .hour(23)
      .minute(59)
      .second(59)
      .format('YYYY-MM-DDTHH:mm:ssZ'),
    site: siteFromPath,
  };

  const [site, bookings] = await Promise.all([
    fetchSite(siteFromPath),
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
    href: `/site/${site.id}/view-availability/week?date=${date}`,
    text: 'Back to week view',
  };

  const canCancelBookings = (await fetchPermissions(site.id)).includes(
    'booking:cancel',
  );

  return (
    <NhsPage
      title={parsedDate.format('dddd D MMMM')}
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
