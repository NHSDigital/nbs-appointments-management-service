import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchClinicalServices,
  fetchFeatureFlag,
  fetchSite,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import { EditSessionDecision } from './edit-session-decision';
import { parseToUkDatetime } from '@services/timeService';

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

  const [site, multipleServicesFlag, clinicalServices] = await Promise.all([
    fetchSite(siteFromPath),
    fetchFeatureFlag('MultipleServices'),
    fetchClinicalServices(),
  ]);

  const parsedDate = parseToUkDatetime(date);

  await assertPermission(siteFromPath, 'availability:setup');

  return (
    <NhsPage
      title={`Change availability for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      site={site}
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
        multipleServicesEnabled={multipleServicesFlag.enabled}
        clinicalServices={clinicalServices}
      ></EditSessionDecision>
    </NhsPage>
  );
};

export default Page;
