import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchSite,
  fetchClinicalServices,
} from '@services/appointmentsService';
import { fetchBookings } from '@services/appointmentsService';
import { NoNotificationsPage } from './no-notifications-page';
import { FetchBookingsRequest } from '@types';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { dateTimeFormat, parseToUkDatetime } from '@services/timeService';
import { notFound } from 'next/navigation';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    page: number;
    cancelledWithoutDetailsCount: number;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date, page, cancelledWithoutDetailsCount } = {
    ...(await searchParams),
  };
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

  const cancelledBookings = bookings.filter(b => b.status === 'Cancelled');

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week?date=${date}`,
    text: 'Back to week view',
  };

  const caption = `${cancelledWithoutDetailsCount} people did not get a cancellation notification as they had no contact details on their booking.`;

  return (
    <NhsPage
      title="People wo did not receive a notification"
      caption={caption}
      backLink={backLink}
      originPage="view-availability-daily-appointments"
      site={site}
      showPrintButton
    >
      <NoNotificationsPage
        bookings={cancelledBookings}
        site={site.id}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
