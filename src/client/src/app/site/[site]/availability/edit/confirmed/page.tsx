import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import { parseToUkDatetime } from '@services/timeService';
import EditSessionConfirmed from './edit-session-confirmed';
import { notFound } from 'next/navigation';
import NhsPage from '@components/nhs-page';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    session: string;
    newlyOrphanedBookingsCount?: number;
    cancelAppointments?: boolean;
    cancelledWithoutDetailsCount?: number;
    chosenAction: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const {
    session,
    date,
    cancelAppointments,
    newlyOrphanedBookingsCount,
    chosenAction,
    cancelledWithoutDetailsCount,
  } = {
    ...(await searchParams),
  };

  if (session === undefined || date === undefined) {
    return notFound();
  }

  const hasBookings = cancelAppointments == undefined ? false : true;

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const changeSessionUpliftedJourneyFlag = await fromServer(
    fetchFeatureFlag('ChangeSessionUpliftedJourney'),
  );

  const [site, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  const parsedDate = parseToUkDatetime(date);

  const newSession: AvailabilitySession = JSON.parse(atob(session));

  let cancelledWithDetailsCount =
    (newlyOrphanedBookingsCount ?? 0) - (cancelledWithoutDetailsCount ?? 0);

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
        updatedSession={newSession}
        site={site}
        date={date}
        hasBookings={hasBookings}
        chosenAction={chosenAction ?? ''}
        newlyOrphanedBookingsCount={newlyOrphanedBookingsCount ?? 0}
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
