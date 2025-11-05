import {
  assertPermission,
  availabilityChangeProposal,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';
import { AvailabilityChangeProposalRequest, SessionSummary } from '@types';
import { toTimeFormat } from '@services/timeService';
import { SessionModificationConfirmation } from '@components/session-modification-confirmation';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    session: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { session, date } = { ...(await searchParams) };

  if (session === undefined || date === undefined) {
    notFound();
  }

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const availabilityRequest: AvailabilityChangeProposalRequest = {
    from: date,
    to: date,
    site: siteFromPath,
    sessionMatcher: {
      from: toTimeFormat(sessionSummary.ukStartDatetime) ?? '',
      until: toTimeFormat(sessionSummary.ukEndDatetime) ?? '',
      services: Object.keys(sessionSummary.totalSupportedAppointmentsByService),
      slotLength: sessionSummary.slotLength,
      capacity: sessionSummary.capacity,
    },
    sessionReplacement: null,
  };

  const [site, clinicalServices, availabilityProposal] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
    fromServer(availabilityChangeProposal(availabilityRequest)),
  ]);

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week/edit-session?session=${session}&date=${date}`,
    text: 'Go back',
  };

  return (
    <NhsTransactionalPage
      title="Are you sure you want to cancel this session?"
      caption="Cancel session"
      originPage="edit-session"
      backLink={backLink}
    >
      <SessionModificationConfirmation
        unsupportedBookingsCount={availabilityProposal.unsupportedBookingsCount}
        clinicalServices={clinicalServices}
        session={session}
        site={site.id}
        date={date}
        mode="cancel"
      />
    </NhsTransactionalPage>
  );
};

export default Page;
