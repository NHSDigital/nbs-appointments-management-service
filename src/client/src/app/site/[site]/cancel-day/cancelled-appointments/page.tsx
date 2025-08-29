import NhsPage from '@components/nhs-page';
import {
  assertFeatureEnabled,
  assertPermission,
  fetchBookings,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { parseToUkDatetime, RFC3339Format } from '@services/timeService';
import { FetchBookingsRequest } from '@types';
import { notFound } from 'next/navigation';
import { DailyAppointmentsPage } from '../../view-availability/daily-appointments/daily-appointments-page';

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
    from: fromDate.format(RFC3339Format),
    to: toDate.format(RFC3339Format),
    site: siteFromPath,
    statuses: ['Cancelled'],
    cancellationReason: 'CancelledBySite',
  };

  const [site, bookings, clinicalServices] = await Promise.all([
    fetchSite(siteFromPath),
    fetchBookings(fetchBookingsRequest, ['Cancelled']),
    fetchClinicalServices(),
  ]);

  return (
    <NhsPage
      title={fromDate.format('dddd D MMMM')}
      caption={site.name}
      originPage="cancel-day-confirmation"
    >
      <DailyAppointmentsPage
        bookings={bookings}
        clinicalServices={clinicalServices}
        displayAction={false}
        site={site.name}
      />
    </NhsPage>
  );
};

export default Page;
