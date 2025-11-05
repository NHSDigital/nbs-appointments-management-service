import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
import EditServicesConfirmed from './edit-services-confirmed';
import { notFound } from 'next/navigation';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    removedServicesSession: string;
    unsupportedBookingsCount?: number;
    cancelAppointments?: boolean;
    cancelledWithoutDetailsCount?: number;
    chosenAction: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const {
    date,
    removedServicesSession: serialisedSession,
    cancelAppointments,
    unsupportedBookingsCount,
    chosenAction,
    cancelledWithoutDetailsCount,
  } = {
    ...(await searchParams),
  };
  const { site: siteFromPath } = { ...(await params) };

  if (date === undefined || serialisedSession === undefined) {
    return notFound();
  }

  const hasBookings = cancelAppointments == undefined ? false : true;

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  const [site, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  const changeSessionUpliftedJourneyFlag = await fromServer(
    fetchFeatureFlag('ChangeSessionUpliftedJourney'),
  );

  const parsedDate = parseToUkDatetime(date);

  const removedServicesSession: AvailabilitySession = JSON.parse(
    atob(serialisedSession),
  );

  const servicesCount = removedServicesSession.services.length;

  let cancelledWithDetailsCount =
    (unsupportedBookingsCount ?? 0) - (cancelledWithoutDetailsCount ?? 0);

  if (cancelledWithDetailsCount < 0) {
    cancelledWithDetailsCount = 0;
  }

  return (
    <NhsPage
      site={site}
      originPage="edit-session"
      title={`Services removed on ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditServicesConfirmed
        removedServicesSession={removedServicesSession}
        site={site}
        date={date}
        clinicalServices={clinicalServices}
        hasBookings={hasBookings}
        servicesCount={servicesCount}
        chosenAction={chosenAction ?? ''}
        unsupportedBookingsCount={unsupportedBookingsCount ?? 0}
        cancelledWithDetailsCount={cancelledWithDetailsCount ?? 0}
        cancelledWithoutDetailsCount={cancelledWithoutDetailsCount ?? 0}
        changeSessionUpliftedJourneyEnabled={
          changeSessionUpliftedJourneyFlag.enabled
        }
      />
    </NhsPage>
  );
};

export default Page;
