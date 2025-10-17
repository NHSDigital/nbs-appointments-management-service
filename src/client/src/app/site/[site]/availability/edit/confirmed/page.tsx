import {
  assertPermission,
  fetchClinicalServices,
  fetchBookings,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { AvailabilitySession, FetchBookingsRequest } from '@types';
import { dateTimeFormat, parseToUkDatetime } from '@services/timeService';
import EditSessionConfirmed from './edit-session-confirmed';
import { notFound } from 'next/navigation';
import NhsPage from '@components/nhs-page';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    updatedSession: string;
    unsupportedBookingsCount?: number;
    cancelAppointments?: boolean;
    chosenAction: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const {
    updatedSession,
    date,
    cancelAppointments,
    unsupportedBookingsCount,
    chosenAction,
  } = {
    ...(await searchParams),
  };

  if (updatedSession === undefined || date === undefined) {
    return notFound();
  }

  console.log('chosenAction', chosenAction);

  const hasBookings = cancelAppointments == undefined ? false : true;

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const changeSessionUpliftedJourneyFlag = await fromServer(
    fetchFeatureFlag('ChangeSessionUpliftedJourney'),
  );

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

  const parsedDate = parseToUkDatetime(date);

  const updatedAvailabilitySession: AvailabilitySession = JSON.parse(
    atob(updatedSession),
  );

  const cancelledWithoutDetailsCount = bookings.filter(
    b =>
      !b.contactDetails ||
      !b.contactDetails.some(cd => cd.type === 'Email' || cd.type === 'Phone'),
  ).length;

  let cancelledWithDetailsCount =
    (unsupportedBookingsCount ?? 0) - cancelledWithoutDetailsCount;

  if (cancelledWithDetailsCount < 0) {
    cancelledWithDetailsCount = 0;
  }

  return (
    <NhsPage
      site={site}
      originPage="edit-session"
      title={`Time and capacity changed for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditSessionConfirmed
        updatedSession={updatedAvailabilitySession}
        site={site}
        date={date}
        hasBookings={hasBookings}
        bookings={bookings}
        chosenAction={chosenAction ?? ''}
        unsupportedBookingsCount={unsupportedBookingsCount ?? 0}
        cancelledWithDetailsCount={cancelledWithDetailsCount ?? 0}
        cancelledWithoutDetailsCount={cancelledWithoutDetailsCount ?? 0}
        clinicalServices={clinicalServices}
        changeSessionUpliftedJourneyEnabled={
          changeSessionUpliftedJourneyFlag.enabled
        }
      />
    </NhsPage>
  );
};

export default Page;
