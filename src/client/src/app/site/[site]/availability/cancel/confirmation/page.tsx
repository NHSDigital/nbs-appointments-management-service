import {
  assertPermission,
  fetchSite,
  availabilityChangeProposal,
  fetchClinicalServices,
} from '@services/appointmentsService';
import { AvailabilityChangeProposalRequest, SessionSummary } from '@types';
import { SessionModificationConfirmation } from '@components/session-modification-confirmation';
import { parseToUkDatetime, toTimeFormat } from '@services/timeService';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

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
    return notFound();
  }
  await assertPermission(siteFromPath, 'availability:setup');
  const parsedDate = parseToUkDatetime(date);
  const site = await fromServer(fetchSite(siteFromPath));
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

  const availabilityProposal = await fromServer(
    availabilityChangeProposal(availabilityRequest),
  );

  const clinicalServices = await fromServer(fetchClinicalServices());
  return (
    <NhsTransactionalPage
      title={`Cancel session for ${parsedDate.format('dddd DD MMMM')}`}
      caption={site.name}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/availability/cancel?session=${session}&date=${date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
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
