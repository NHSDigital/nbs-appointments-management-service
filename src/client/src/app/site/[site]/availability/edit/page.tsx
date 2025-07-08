import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import EditSessionTimeAndCapacityForm from './edit-session-time-and-capacity-form';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';

type PageProps = {
  searchParams: Promise<{
    date: string;
    session: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const [search, props] = await Promise.all([searchParams, params]);

  await assertPermission(props.site, 'availability:setup');
  const site = await fetchSite(props.site);
  const date = parseToUkDatetime(search.date);
  const sessionSummary: SessionSummary = JSON.parse(atob(search.session));

  return (
    <NhsPage
      title={`Edit time and capacity for ${date.format('DD MMMM YYYY')}`}
      caption={'Edit session'}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/view-availability/week/edit-session?session=${search.session}&date=${search.date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <EditSessionTimeAndCapacityForm
        date={search.date}
        site={site}
        existingSession={sessionSummary}
      />
    </NhsPage>
  );
};

export default Page;
