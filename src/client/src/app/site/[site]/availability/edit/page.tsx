import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import EditSessionTimeAndCapacityForm from './edit-session-time-and-capacity-form';
import { parseToUkDatetime } from '@services/timeService';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@components/nhs-transactional-page';

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
  const site = await fetchSite(siteFromPath);

  const sessionSummary: SessionSummary = JSON.parse(atob(session));

  return (
    <NhsTransactionalPage
      title={`Edit time and capacity for ${parsedDate.format('DD MMMM YYYY')}`}
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
    </NhsTransactionalPage>
  );
};

export default Page;
