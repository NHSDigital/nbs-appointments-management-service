import {
  assertPermission,
  fetchSite,
  fetchClinicalServices,
  assertFeatureEnabled,
  proposeAnAvailabilityChange,
} from '@services/appointmentsService';
import { AvailabilitySession, SessionSummary } from '@types';
import { SessionModificationConfirmation } from '@components/session-modification-confirmation';
import { DayJsType, parseToUkDatetime } from '@services/timeService';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

type SearchParams = {
  date: string;
  session: string;
};

type ParsedParams = {
  date: DayJsType;
  session: AvailabilitySession;
};

type PageProps = {
  searchParams?: Promise<SearchParams>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date, session } = await parseSearchParams(searchParams);

  await assertPermission(siteFromPath, 'availability:setup');
  await assertFeatureEnabled('CancelSessionUpliftedJourney');

  const [site, availabilityProposal, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(
      proposeAnAvailabilityChange(siteFromPath, date, date, session, null),
    ),
    fromServer(fetchClinicalServices()),
  ]);

  return (
    <NhsTransactionalPage
      title={`Cancel session for ${date}`}
      caption={site.name}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/availability/cancel?session=${session}&date=${date}`,
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
        site={site.id}
        date={date}
        mode="cancel"
      />
    </NhsTransactionalPage>
  );
};

const parseSearchParams = async (
  params?: Promise<SearchParams>,
): Promise<ParsedParams> => {
  const { session, date } = { ...(await params) };
  if (session === undefined || date === undefined) {
    notFound();
  }

  const parsedDate = parseToUkDatetime(date);
  // TODO: Wouldn't it be better to pass AvailabilitySession rather than SessionSummary?
  const parsedSession: SessionSummary = JSON.parse(atob(session));

  const availabilitySession: AvailabilitySession = {
    from: parsedSession.ukStartDatetime,
    until: parsedSession.ukEndDatetime,
    services: Object.keys(parsedSession.totalSupportedAppointmentsByService),
    slotLength: parsedSession.slotLength,
    capacity: parsedSession.capacity,
  };

  return { date: parsedDate, session: availabilitySession };
};

export default Page;
