import { assertPermission, fetchSite } from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import NhsPage from '@components/nhs-page';
import dayjs from 'dayjs';
import EditSessionConfirmed from './edit-session-confirmed';
import { notFound } from 'next/navigation';

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
    notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');

  const site = await fetchSite(siteFromPath);

  const updatedAvailabilitySession: AvailabilitySession = JSON.parse(
    atob(updatedSession),
  );

  return (
    <NhsPage
      originPage="edit-session"
      title={`Edit time and capacity for ${dayjs(date, 'YYYY-MM-DD').format('DD MMMM YYYY')}`}
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
      />
    </NhsPage>
  );
};

export default Page;
