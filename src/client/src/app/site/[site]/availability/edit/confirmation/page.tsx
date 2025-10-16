import {
  assertPermission,
  fetchSite,
  availabilityChangeProposal,
  fetchClinicalServices,
} from '@services/appointmentsService';
import {
  AvailabilityChangeProposalRequest,
  SessionSummary,
  Session,
} from '@types';
import { EditSessionConfirmation } from './edit-session-confirmation';
import { parseToUkDatetime, toTimeFormat } from '@services/timeService';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@compon121ents/nhs-transactional-page';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    session: string;
    sessionToEdit: string;
    updatedSession: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { session, date, sessionToEdit, updatedSession } = {
    ...(await searchParams),
  };
  if (
    session === undefined ||
    date === undefined ||
    sessionToEdit === undefined ||
    updatedSession === undefined
  ) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');
  const parsedDate = parseToUkDatetime(date);
  const site = await fromServer(fetchSite(siteFromPath));
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const newSessionDetails: Session = JSON.parse(atob(updatedSession));
  const existingSession: Session = JSON.parse(atob(sessionToEdit));
  const availabilityRequest: AvailabilityChangeProposalRequest = {
    from: date,
    to: date,
    site: siteFromPath,
    sessionMatcher: {
      from: toTimeFormat(sessionSummary.ukStartDatetime) ?? '',
      until: toTimeFormat(sessionSummary.ukEndDatetime) ?? '',
      services: newSessionDetails.services,
      slotLength: sessionSummary.slotLength,
      capacity: sessionSummary.capacity,
    },
    sessionReplacement: {
      from: `${newSessionDetails.startTime.hour.toString().padStart(2, '0')}:${newSessionDetails.startTime.minute.toString().padStart(2, '0')}`,
      until: `${newSessionDetails.endTime.hour.toString().padStart(2, '0')}:${newSessionDetails.endTime.minute.toString().padStart(2, '0')}`,
      services: newSessionDetails.services,
      slotLength: newSessionDetails.slotLength,
      capacity: newSessionDetails.capacity,
    },
  };

  const availabilityProposal = await fromServer(
    availabilityChangeProposal(availabilityRequest),
  );
  const clinicalServices = await fromServer(fetchClinicalServices());

  return (
    <NhsTransactionalPage
      title={`New time and capacity for ${parsedDate.format('dddd DD MMMM')}`}
      caption={site.name}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/availability/edit?session=${session}&date=${date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <EditSessionConfirmation
        unsupportedBookingsCount={availabilityProposal.unsupportedBookingsCount}
        clinicalServices={clinicalServices}
        session={session}
        newSessionDetails={newSessionDetails}
        sessionToEdit={existingSession}
        site={site.id}
        date={date}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
