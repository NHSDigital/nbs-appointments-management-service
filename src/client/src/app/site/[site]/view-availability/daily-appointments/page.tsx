import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
  fetchClinicalServices,
} from '@services/appointmentsService';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import { DailyAppointmentsPage } from './daily-appointments-page';
import { FetchBookingsRequest } from '@types';
import { Tab, Tabs } from '@nhsuk-frontend-components';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { dateTimeFormat, parseToUkDatetime } from '@services/timeService';
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
    return notFound();
  }

  await assertPermission(siteFromPath, 'booking:view-detail');

  const fromDate = parseToUkDatetime(date);
  const toDate = fromDate.endOf('day');

  const fetchBookingsRequest: FetchBookingsRequest = {
    from: fromDate.format(dateTimeFormat),
    to: toDate.format(dateTimeFormat),
    site: siteFromPath,
  };

  const [site, bookings, clinicalServices] = await Promise.all([
    fetchSite(siteFromPath),
    fetchBookings(fetchBookingsRequest, ['Booked', 'Cancelled']),
    fetchClinicalServices(),
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
      ? `${orphanedAppointments.length} booked appointments are affected due to an edit to your availability. These bookings are still scheduled until you click "Cancel".`
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
      title={fromDate.format('dddd D MMMM')}
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
            clinicalServices={clinicalServices}
          />
        </Tab>
        <Tab title="Cancelled">
          <DailyAppointmentsPage
            bookings={cancelledBookings}
            site={site.id}
            displayAction={false}
            clinicalServices={clinicalServices}
          />
        </Tab>
        <Tab title="Manual Cancellations">
          <DailyAppointmentsPage
            bookings={orphanedAppointments}
            site={site.id}
            displayAction={canCancelBookings}
            message={orphanedMessage}
            clinicalServices={clinicalServices}
          />
        </Tab>
      </Tabs>
    </NhsPage>
  );
};

export default Page;
