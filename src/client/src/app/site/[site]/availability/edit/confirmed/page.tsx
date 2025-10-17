import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import { parseToUkDatetime } from '@services/timeService';
import EditSessionConfirmed from './edit-session-confirmed';
import { notFound } from 'next/navigation';
import NhsPage from '@components/nhs-page';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    updatedSession: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { updatedSession, date } = { ...(await searchParams) };

  if (updatedSession === undefined || date === undefined) {
    return notFound();
  }

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));
  const [site, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  const parsedDate = parseToUkDatetime(date);

  const updatedAvailabilitySession: AvailabilitySession = JSON.parse(
    atob(updatedSession),
  );

  return (
    <NhsPage
      site={site}
      originPage="edit-session"
      title={`Edit time and capacity for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditSessionConfirmed
        updatedSession={updatedAvailabilitySession}
        site={site}
        date={date}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
