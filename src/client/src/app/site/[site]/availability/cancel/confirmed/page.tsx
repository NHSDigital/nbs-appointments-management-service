import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import CancellationConfirmed from './cancellation-confirmed';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { parseToUkDatetime } from '@services/timeService';
import NhsPage from '@components/nhs-page';
import { SessionModificationConfirmed } from '@components/session-modification-confirmed';
import fromServer from '@server/fromServer';
import { SessionSummary, SessionModificationAction } from '@types';

type PageProps = {
  searchParams?: Promise<{
    session: string;
    date: string;
    chosenAction: SessionModificationAction;
    bookingsCanceledWithoutDetails: number;
    unsupportedBookingsCount: number;
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
    chosenAction,
    unsupportedBookingsCount,
    bookingsCanceledWithoutDetails,
  } = {
    ...(await searchParams),
  };

  const parsedUnsupportedBookingsCount = Number(unsupportedBookingsCount ?? 0);
  const parsedBookingsCanceledWithoutDetailsCount = Number(
    bookingsCanceledWithoutDetails ?? 0,
  );

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  if (session === undefined || date === undefined) {
    notFound();
  }

  const cancelSessionUpliftedJourneyFlag = await fromServer(
    fetchFeatureFlag('CancelSessionUpliftedJourney'),
  );

  const [site, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week/?date=${date}`,
    text: 'Back to week view',
  };

  const sessionSummary: SessionSummary = JSON.parse(atob(session));

  return (
    <>
      {cancelSessionUpliftedJourneyFlag.enabled ? (
        <NhsPage
          site={site}
          title={`Session cancelled for ${parseToUkDatetime(date).format('dddd DD MMMM')}`}
          caption={`${site.name}`}
          originPage="edit-session"
          backLink={backLink}
        >
          <SessionModificationConfirmed
            sessionSummary={sessionSummary}
            date={date}
            siteId={site.id}
            clinicalServices={clinicalServices}
            modificationAction={chosenAction as SessionModificationAction}
            unsupportedBookingsCount={parsedUnsupportedBookingsCount}
            bookingsCanceledWithoutDetails={
              parsedBookingsCanceledWithoutDetailsCount
            }
          />
        </NhsPage>
      ) : (
        <NhsPage
          site={site}
          title={`Cancelled session for ${parseToUkDatetime(date).format('DD MMMM YYYY')}`}
          caption={`${site.name}`}
          originPage="edit-session"
          backLink={backLink}
        >
          <CancellationConfirmed
            session={session}
            date={date}
            site={site.id}
            clinicalServices={clinicalServices}
          />
        </NhsPage>
      )}
    </>
  );
};

export default Page;
