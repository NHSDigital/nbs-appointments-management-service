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
  parseDateAndTimeComponentsToUkDateTime,
  parseToUkDatetime,
  toTimeFormat,
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
    existingSession: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { session, date, sessionToEdit, existingSession } = {
    ...(await searchParams),
  };
  if (
    session === undefined ||
    date === undefined ||
    sessionToEdit === undefined ||
    existingSession === undefined
  ) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');
  const parsedDate = parseToUkDatetime(date);
  const site = await fromServer(fetchSite(siteFromPath));
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const newSessionDetails: Session = JSON.parse(atob(sessionToEdit));
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

  const parsedSession: SessionSummary = {
    capacity: newSessionDetails.capacity,
    maximumCapacity: sessionSummary.maximumCapacity,
    slotLength: newSessionDetails.slotLength,
    totalSupportedAppointments: sessionSummary.totalSupportedAppointments,
    totalSupportedAppointmentsByService:
      sessionSummary.totalSupportedAppointmentsByService,
    ukStartDatetime: parseDateAndTimeComponentsToUkDateTime(
      date,
      newSessionDetails.startTime,
    ).format(dateTimeFormat),
    ukEndDatetime: parseDateAndTimeComponentsToUkDateTime(
      date,
      newSessionDetails.endTime,
    ).format(dateTimeFormat),
  };

  const existing: Session = JSON.parse(atob(existingSession));

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
        unsupportedBookingsCount={availabilityProposal.unsupportedBookingsCount}
        clinicalServices={clinicalServices}
        session={btoa(JSON.stringify(parsedSession))}
        newSessionDetails={newSessionDetails}
        sessionToEdit={existing}
        site={site.id}
        date={date}
        mode="edit"
      />
    </NhsTransactionalPage>
  );
};

export default Page;
