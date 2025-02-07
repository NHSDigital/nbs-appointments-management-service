import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import EditSessionTimeAndCapacityForm from './edit-session-time-and-capacity-form';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
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
  const { session, date: dateFromParams } = { ...(await searchParams) };
  if (session === undefined || dateFromParams === undefined) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');
  const date = parseToUkDatetime(dateFromParams);
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
      />
    </NhsPage>
  );
};

export default Page;
