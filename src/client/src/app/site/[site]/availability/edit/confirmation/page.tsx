import {
  assertPermission,
  fetchSite,
  proposeAnAvailabilityChange,
  fetchClinicalServices,
  assertFeatureEnabled,
} from '@services/appointmentsService';
import { SessionSummary, AvailabilitySession } from '@types';
import { parseToUkDatetime, DayJsType } from '@services/timeService';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';
import { SessionModificationConfirmation } from '@components/session-modification-confirmation';

type SearchParams = {
  date: string;
  session: string;
  sessionToEdit: string;
};

type ParsedParams = {
  date: DayJsType;
  session: AvailabilitySession;
  sessionToEdit: AvailabilitySession;
};

type PageProps = {
  searchParams?: Promise<{
    date: string;
    // TODO: What is the difference between Session and Session to edit?
    // If the 2nd session is the one "to edit", why do we need the first?
    session: string;
    sessionToEdit: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date, session, sessionToEdit } =
    await parseSearchParams(searchParams);

  await assertPermission(siteFromPath, 'availability:setup');
  await assertFeatureEnabled('ChangeSessionUpliftedJourney');

  const [site, availabilityProposal, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(
      proposeAnAvailabilityChange(
        siteFromPath,
        date,
        date,
        session,
        sessionToEdit,
      ),
    ),
    fromServer(fetchClinicalServices()),
  ]);

  return (
    <NhsTransactionalPage
      title={`New time and capacity for ${date.format('dddd DD MMMM')}`}
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
        newSession={sessionToEdit}
        site={site.id}
        date={date}
        mode="edit"
      />
    </NhsTransactionalPage>
  );
};

const parseSearchParams = async (
  params?: Promise<SearchParams>,
): Promise<ParsedParams> => {
  const { session, date, sessionToEdit } = { ...(await params) };
  if (
    session === undefined ||
    date === undefined ||
    sessionToEdit === undefined
  ) {
    notFound();
  }

  const parsedDate = parseToUkDatetime(date);
  // TODO: Wouldn't it be better to pass AvailabilitySession rather than SessionSummary?
  const parsedFirstSessionSummary: SessionSummary = JSON.parse(atob(session));
  const parsedSecondSessionSummary: SessionSummary = JSON.parse(atob(session));

  const firstAvailabilitySession: AvailabilitySession = {
    from: parsedFirstSessionSummary.ukStartDatetime,
    until: parsedFirstSessionSummary.ukEndDatetime,
    services: Object.keys(
      parsedFirstSessionSummary.totalSupportedAppointmentsByService,
    ),
    slotLength: parsedFirstSessionSummary.slotLength,
    capacity: parsedFirstSessionSummary.capacity,
  };

  const secondAvailabilitySession: AvailabilitySession = {
    from: parsedSecondSessionSummary.ukStartDatetime,
    until: parsedSecondSessionSummary.ukEndDatetime,
    services: Object.keys(
      parsedSecondSessionSummary.totalSupportedAppointmentsByService,
    ),
    slotLength: parsedSecondSessionSummary.slotLength,
    capacity: parsedSecondSessionSummary.capacity,
  };

  return {
    date: parsedDate,
    session: firstAvailabilitySession,
    sessionToEdit: secondAvailabilitySession,
  };
};

export default Page;
