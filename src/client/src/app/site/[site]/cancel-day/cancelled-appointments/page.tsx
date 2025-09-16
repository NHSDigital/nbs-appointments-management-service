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

  await assertFeatureEnabled('CancelDay');
  await assertPermission(siteFromPath, 'booking:view-detail');

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
    fetchSite(siteFromPath),
    fetchBookings(fetchBookingsRequest, ['Cancelled']),
    fetchClinicalServices(),
  ]);

  const bookingsWithoutContactDetails = bookings.filter(
    b => b.contactDetails === null || b.contactDetails?.length === 0,
  );

  return (
    <NhsPage
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
