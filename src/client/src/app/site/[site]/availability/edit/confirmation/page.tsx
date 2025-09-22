import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import { EditSessionConfirmation } from './edit-session-confirmation';
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
      title={`New time and capacity for ${parsedDate.format('dddd DD MMMM')}`}
      caption={site.name}
      originPage="edit-session"
      // backLink={{
      //   href: `/site/${site.id}/view-availability/week/edit-session?session=${session}&date=${date}`,
      //   renderingStrategy: 'server',
      //   text: 'Go back',
      // }}
    >
      <EditSessionConfirmation />
    </NhsTransactionalPage>
  );
};

export default Page;
