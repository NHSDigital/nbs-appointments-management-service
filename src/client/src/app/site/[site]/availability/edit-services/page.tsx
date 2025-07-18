import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { SessionSummary } from '@types';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
import EditServicesForm from './edit-services-form';
import { notFound } from 'next/navigation';

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
  const { date, session } = { ...(await searchParams) };
  const { site: siteFromPath } = { ...(await params) };

  if (date === undefined || session === undefined) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');

  const [site, clinicalServices] = await Promise.all([
    fetchSite(siteFromPath),
    fetchClinicalServices(),
  ]);

  const parsedDate = parseToUkDatetime(date);
  const sessionSummary: SessionSummary = JSON.parse(atob(session));

  return (
    <NhsPage
      title={`Remove services for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={'Remove services'}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/view-availability/week/edit-session?session=${session}&date=${date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <EditServicesForm
        date={date}
        site={site}
        existingSession={sessionSummary}
        clinicalServices={clinicalServices}
      ></EditServicesForm>
    </NhsPage>
  );
};

export default Page;
