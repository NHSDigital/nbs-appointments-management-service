import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import dayjs from 'dayjs';
import { notFound } from 'next/navigation';
import { EditSessionDecision } from './edit-session-decision';
import { InsetText } from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';

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
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:setup');
  const date = dayjs(searchParams.date, 'YYYY-MM-DD');

  if (searchParams.session === undefined || searchParams.date === undefined) {
    notFound();
  }

  const sessionSummary: SessionSummary = JSON.parse(atob(searchParams.session));

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
      <SessionSummaryTable sessionSummaries={[sessionSummary]} />
      <InsetText>
        <p>
          You can only reduce time and/or capacity from this screen. If you want
          to increase availability for this day, you must create a new session.
        </p>
      </InsetText>
      <EditSessionDecision
        site={site}
        sessionSummary={searchParams.session}
        date={searchParams.date}
      ></EditSessionDecision>
    </NhsPage>
  );
};

export default Page;
