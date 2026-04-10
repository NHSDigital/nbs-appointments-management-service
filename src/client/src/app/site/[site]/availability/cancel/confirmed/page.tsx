import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
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
    cancelledWithoutDetailsCount: number;
    newlyUnsupportedBookingsCount: number;
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
    newlyUnsupportedBookingsCount,
    cancelledWithoutDetailsCount,
  } = {
    ...(await searchParams),
  };

  const parsedNewlyUnsupportedBookingsCount = Number(
    newlyUnsupportedBookingsCount ?? 0,
  );
  const parsedBookingsCanceledWithoutDetailsCount = Number(
    cancelledWithoutDetailsCount ?? 0,
  );

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  if (session === undefined || date === undefined) {
    notFound();
  }

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
        newlyUnsupportedBookingsCount={parsedNewlyUnsupportedBookingsCount}
        bookingsCanceledWithoutDetails={
          parsedBookingsCanceledWithoutDetailsCount
        }
      />
    </NhsPage>
  );
};

export default Page;
