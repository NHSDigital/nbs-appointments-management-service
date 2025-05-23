import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import EditSessionTimeAndCapacityForm from './edit-session-time-and-capacity-form';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';

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
  await assertPermission(params.site, 'availability:setup');
  const site = await fetchSite(params.site);
  const date = parseToUkDatetime(searchParams.date);
  const sessionSummary: SessionSummary = JSON.parse(atob(searchParams.session));

  return (
    <NhsPage
      title={`Edit time and capacity for ${date.format('DD MMMM YYYY')}`}
      caption={'Edit session'}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/view-availability/week/edit-session?session=${searchParams.session}&date=${searchParams.date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <EditSessionTimeAndCapacityForm
        date={searchParams.date}
        site={site}
        existingSession={sessionSummary}
      />
    </NhsPage>
  );
};

export default Page;
