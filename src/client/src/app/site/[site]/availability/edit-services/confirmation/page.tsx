import {
  assertPermission,
  fetchSite,
  availabilityChangeProposal,
  fetchClinicalServices,
} from '@services/appointmentsService';
import {
  AvailabilityChangeProposalRequest,
  SessionSummary,
  AvailabilitySession,
} from '@types';
import { parseToUkDatetime, toTimeFormat } from '@services/timeService';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';
import { EditServicesConfirmationPage } from './edit-services-confirmation';
import { Suspense } from 'react';
import { Spinner } from '@components/nhsuk-frontend';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    removedServicesSession: string;
    session: string;
    sessionToEdit: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { removedServicesSession, date, sessionToEdit, session } = {
    ...(await searchParams),
  };

  if (
    removedServicesSession === undefined ||
    date === undefined ||
    sessionToEdit === undefined ||
    session === undefined
  ) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');
  const parsedDate = parseToUkDatetime(date);
  const site = await fromServer(fetchSite(siteFromPath));
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const removedServicesSessionDetails: AvailabilitySession = JSON.parse(
    atob(removedServicesSession),
  );

  const newSessionDetails: AvailabilitySession = JSON.parse(
    atob(decodeURIComponent(sessionToEdit)),
  );

  newSessionDetails.services = newSessionDetails.services.filter(
    service => !removedServicesSessionDetails.services.includes(service),
  );

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
      from: removedServicesSessionDetails.from,
      until: removedServicesSessionDetails.until,
      services: newSessionDetails.services,
      slotLength: removedServicesSessionDetails.slotLength,
      capacity: removedServicesSessionDetails.capacity,
    },
  };

  const availabilityProposal = await fromServer(
    availabilityChangeProposal(availabilityRequest),
  );

  const clinicalServices = await fromServer(fetchClinicalServices());

  return (
    <NhsTransactionalPage
      title={`Remove services for ${parsedDate.format('dddd DD MMMM')}`}
      caption={site.name}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/availability/edit-services?session=${session}&date=${date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <Suspense fallback={<Spinner />}>
        <EditServicesConfirmationPage
          unsupportedBookingsCount={
            availabilityProposal.unsupportedBookingsCount
          }
          clinicalServices={clinicalServices}
          session={session}
          newSession={btoa(JSON.stringify(newSessionDetails))}
          removedServicesSession={removedServicesSession}
          site={site.id}
          date={date}
        />
      </Suspense>
    </NhsTransactionalPage>
  );
};

export default Page;
