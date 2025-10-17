import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
  fetchClinicalServices,
} from '@services/appointmentsService';
import { fetchBookings } from '@services/appointmentsService';
import { NoNotificationsPage } from './no-notifications-page';
import { FetchBookingsRequest } from '@types';
import { Tab, Tabs } from '@nhsuk-frontend-components';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { dateTimeFormat, parseToUkDatetime } from '@services/timeService';
import { notFound } from 'next/navigation';
import fromServer from '@server/fromServer';

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

  await fromServer(assertPermission(siteFromPath, 'booking:view-detail'));
  const fromDate = parseToUkDatetime(date);
  const toDate = fromDate.endOf('day');

  const fetchBookingsRequest: FetchBookingsRequest = {
    from: fromDate.format(dateTimeFormat),
    to: toDate.format(dateTimeFormat),
    site: siteFromPath,
  };

  const [site, bookings, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchBookings(fetchBookingsRequest, ['Booked', 'Cancelled'])),
    fromServer(fetchClinicalServices()),
  ]);

  const scheduledBookings = bookings.filter(
    b => b.status === 'Booked' && b.availabilityStatus !== 'Orphaned',
  );
  const cancelledBookings = bookings.filter(b => b.status === 'Cancelled');
  const orphanedAppointments = bookings.filter(
    b => b.availabilityStatus === 'Orphaned',
  );

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week?date=${date}`,
    text: 'Back to week view',
  };

  const canCancelBookings = (
    await fromServer(fetchPermissions(site.id))
  ).includes('booking:cancel');

  return (
    <NhsPage
      title={fromDate.format('dddd D MMMM')}
      caption={site.name}
      backLink={backLink}
      originPage="view-availability-daily-appointments"
      site={site}
      showPrintButton
    >
      <NoNotificationsPage
        bookings={cancelledBookings}
        site={site.id}
        displayAction={false}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
