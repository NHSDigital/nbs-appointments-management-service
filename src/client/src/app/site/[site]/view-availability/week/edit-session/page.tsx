import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import { EditSessionDecision } from './edit-session-decision';
import { parseToUkDatetime } from '@services/timeService';
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
  const cancelSessionUpliftedJourneyFlag = await fromServer(
    fetchFeatureFlag('CancelSessionUpliftedJourney'),
  );
  const { site: siteFromPath } = { ...(await params) };
  const { session, date } = { ...(await searchParams) };
  if (session === undefined || date === undefined) {
    return notFound();
  }

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const [site, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  const parsedDate = parseToUkDatetime(date);

  return (
    <NhsTransactionalPage
      title={`Change availability for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        renderingStrategy: 'server',
        href: `/site/${site.id}/view-availability/week/?date=${date}`,
        text: 'Go back',
      }}
      originPage="edit-session"
    >
      <EditSessionDecision
        site={site}
        sessionSummary={session}
        date={date}
        clinicalServices={clinicalServices}
        cancelSessionUpliftedJourneyFlag={
          cancelSessionUpliftedJourneyFlag.enabled
        }
      ></EditSessionDecision>
    </NhsTransactionalPage>
  );
};

export default Page;
