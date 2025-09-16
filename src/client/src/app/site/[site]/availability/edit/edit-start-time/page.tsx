import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { parseToUkDatetime } from '@services/timeService';
import { AvailabilitySession, SessionSummary } from '@types';
import { notFound } from 'next/navigation';
import EditSessionStartTimeForm from './edit-session-start-time-form';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    site: string;
    existingSession: string;
    updatedSession: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date, existingSession, updatedSession } = { ...(await searchParams) };
  if (
    date === undefined ||
    existingSession === undefined ||
    updatedSession === undefined
  ) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');
  const parsedDate = parseToUkDatetime(date);
  const site = await fetchSite(siteFromPath);

  const existing: SessionSummary = JSON.parse(atob(existingSession));
  const updated: AvailabilitySession = JSON.parse(atob(updatedSession));

  return (
    <NhsPage
      title={`Edit time and capacity for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      originPage="edit-session-start-time"
      backLink={{
        href: `/site/${site.id}/availability/edit?session=${existingSession}&date=${date}`,
        renderingStrategy: 'server',
        text: 'Back',
      }}
    >
      <EditSessionStartTimeForm
        date={date}
        existingSession={existing}
        updatedSession={updated}
        site={site}
      />
    </NhsPage>
  );
};

export default Page;
