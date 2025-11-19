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
import {
  dateTimeFormat,
  parseToUkDatetime,
  toTimeFormat,
  parseDateAndTimeComponentsToUkDateTime,
} from '@services/timeService';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';
import { SessionModificationConfirmation } from '@components/session-modification-confirmation';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    session: string;
    sessionToEdit: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { session, date, sessionToEdit } = { ...(await searchParams) };
  if (
    session === undefined ||
    date === undefined ||
    sessionToEdit === undefined
  ) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');
  const parsedDate = parseToUkDatetime(date);
  const site = await fromServer(fetchSite(siteFromPath));
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const newSessionDetails: Session = JSON.parse(atob(sessionToEdit));
  const newSessionSummary: SessionSummary = {
    ...sessionSummary,
    capacity: newSessionDetails.capacity,
    ukStartDatetime: parseDateAndTimeComponentsToUkDateTime(
      date,
      newSessionDetails.startTime,
    ).format(dateTimeFormat),
    ukEndDatetime: parseDateAndTimeComponentsToUkDateTime(
      date,
      newSessionDetails.endTime,
    ).format(dateTimeFormat),
  };

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
    sessionReplacement: {
      from: `${newSessionDetails.startTime.hour}:${newSessionDetails.startTime.minute}`,
      until: `${newSessionDetails.endTime.hour}:${newSessionDetails.endTime.minute}`,
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
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={
          availabilityProposal.newlyUnsupportedBookingsCount
        }
        clinicalServices={clinicalServices}
        session={session}
        newSession={newSessionSummary}
        site={site.id}
        date={date}
        mode="edit"
      />
    </NhsTransactionalPage>
  );
};

export default Page;
