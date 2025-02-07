import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { notFound } from 'next/navigation';
import { EditSessionDecision } from './edit-session-decision';

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

  return (
    <NhsPage
      title={`Change availability for ${dayjs(date, 'YYYY-MM-DD').format('DD MMMM YYYY')}`}
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
      ></EditSessionDecision>
    </NhsPage>
  );
};

export default Page;
