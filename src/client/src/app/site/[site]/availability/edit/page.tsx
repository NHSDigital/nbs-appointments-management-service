import { assertPermission, fetchSite } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import EditSessionTimeAndCapacityForm from './edit-session-time-and-capacity-form';
import NhsPage from '@components/nhs-page';
import dayjs from 'dayjs';

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

  const sessionSummary: SessionSummary = JSON.parse(atob(searchParams.session));

  return (
    <NhsPage
      title={`Edit time and capacity for ${date.format('DD MMMM YYYY')}`}
      caption={'Edit session'}
      originPage="edit-session"
    >
      <EditSessionTimeAndCapacityForm
        date={searchParams.date}
        site={site}
        existingSession={sessionSummary}
        existingSessionStart={dayjs(sessionSummary.start).format('HH:mm')}
        existingSessionEnd={dayjs(sessionSummary.end).format('HH:mm')}
      />
    </NhsPage>
  );
};

export default Page;
