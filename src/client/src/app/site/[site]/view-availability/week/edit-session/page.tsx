import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { notFound } from 'next/navigation';
import { EditSessionDecision } from './edit-session-decision';

type PageProps = {
  searchParams: {
    date: string;
    session: string;
  };
  params: {
    site: string;
  };
};

const Page = async ({ searchParams, params }: PageProps) => {
  const [site] = await Promise.all([
    fetchSite(params.site),
    assertPermission(params.site, 'availability:setup'),
  ]);

  const date = dayjs(searchParams.date, 'YYYY-MM-DD');

  if (searchParams.session === undefined || searchParams.date === undefined) {
    notFound();
  }

  return (
    <NhsPage
      title={`Change availability for ${date.format('DD MMMM YYYY')}`}
      caption={site.name}
      site={site}
      backLink={{
        renderingStrategy: 'server',
        href: `/site/${params.site}/view-availability/week/?date=${searchParams.date}`,
        text: 'Go back',
      }}
      originPage="edit-session"
    >
      <EditSessionDecision
        site={site}
        sessionSummary={searchParams.session}
        date={searchParams.date}
      ></EditSessionDecision>
    </NhsPage>
  );
};

export default Page;
