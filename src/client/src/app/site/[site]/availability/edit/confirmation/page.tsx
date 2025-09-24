import {
  assertPermission,
  fetchSite,
  availabilityChangeProposal,
  fetchClinicalServices,
} from '@services/appointmentsService';
import { AvailabilityChangeProposalRequest, SessionSummary } from '@types';
import { EditSessionConfirmation } from './edit-session-confirmation';
import { parseToUkDatetime } from '@services/timeService';
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
  const availabilityRequest: AvailabilityChangeProposalRequest = {
    //TODO create actual request
    from: '2025-10-26',
    to: '2025-10-26',
    site: '6877d86e-c2df-4def-8508-e1eccf0ea6be',
    sessionMatcher: {
      from: '10:00',
      until: '17:00',
      services: ['RSV:Adult'],
      slotLength: 5,
      capacity: 5,
    },
    sessionReplacement: {
      from: '12:00',
      until: '14:00',
      services: ['RSV:Adult', 'COVID:5_11'],
      slotLength: 5,
      capacity: 5,
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
        unsuportedBookingsCount={availabilityProposal.unsupportedBookingsCount}
        clinicalServices={clinicalServices}
        session={session}
        site={site.id}
        date={date}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
