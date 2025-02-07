import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import EditSessionTimeAndCapacityForm from './edit-session-time-and-capacity-form';
import NhsPage from '@components/nhs-page';
import dayjs from 'dayjs';
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
  const { site: siteFromPath } = { ...(await params) };
  const { session, date } = { ...(await searchParams) };
  if (session === undefined || date === undefined) {
    notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');

  const site = await fetchSite(siteFromPath);

  const sessionSummary: SessionSummary = JSON.parse(atob(session));

  return (
    <NhsPage
      title={`Edit time and capacity for ${dayjs(date, 'YYYY-MM-DD').format('DD MMMM YYYY')}`}
      caption={'Edit session'}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/view-availability/week/edit-session?session=${session}&date=${date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <EditSessionTimeAndCapacityForm
        date={date}
        site={site}
        existingSession={sessionSummary}
        existingSessionStart={dayjs(sessionSummary.start).format('HH:mm')}
        existingSessionEnd={dayjs(sessionSummary.end).format('HH:mm')}
      />
    </NhsPage>
  );
};

export default Page;
