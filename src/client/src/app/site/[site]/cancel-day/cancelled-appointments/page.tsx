import NhsPage from '@components/nhs-page';
import {
  assertFeatureEnabled,
  assertPermission,
  fetchBookings,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { dateTimeFormat, parseToUkDatetime } from '@services/timeService';
import { FetchBookingsRequest } from '@types';
import { notFound } from 'next/navigation';
import CancelledAppointments from './cancelled-appointments';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    page: number;
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

  await fromServer(assertFeatureEnabled('CancelDay'));
  await fromServer(assertPermission(siteFromPath, 'booking:view-detail'));

  const fromDate = parseToUkDatetime(date);
  const toDate = fromDate.endOf('day');

  const fetchBookingsRequest: FetchBookingsRequest = {
    from: fromDate.format(dateTimeFormat),
    to: toDate.format(dateTimeFormat),
    site: siteFromPath,
    statuses: ['Cancelled'],
    cancellationReason: 'CancelledBySite',
  };

  const [site, bookings, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchBookings(fetchBookingsRequest, ['Cancelled'])),
    fromServer(fetchClinicalServices()),
  ]);

  const bookingsWithoutContactDetails = bookings.filter(
    b => b.contactDetails === null || b.contactDetails?.length === 0,
  );

  return (
    <NhsPage
      site={site}
      title={fromDate.format('dddd D MMMM')}
      caption={site.name}
      originPage="cancel-day-confirmation"
    >
      <CancelledAppointments
        bookings={bookingsWithoutContactDetails}
        clinicalServices={clinicalServices}
        site={site.name}
      />
    </NhsPage>
  );
};

export default Page;
