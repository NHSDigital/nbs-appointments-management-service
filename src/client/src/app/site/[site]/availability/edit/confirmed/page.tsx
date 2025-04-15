import { assertPermission, fetchSite } from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import NhsPage from '@components/nhs-page';
import dayjs from 'dayjs';
import EditSessionConfirmed from './edit-session-confirmed';
import { dayStringFormat } from '@services/timeService';

type PageProps = {
  searchParams: {
    date: string;
    updatedSession: string;
  };
  params: {
    site: string;
  };
};

const Page = async ({ searchParams, params }: PageProps) => {
  await assertPermission(params.site, 'availability:setup');
  const site = await fetchSite(params.site);

  //TODO refactor!!
  const date = dayjs(searchParams.date, dayStringFormat);

  const updatedSession: AvailabilitySession = JSON.parse(
    atob(searchParams.updatedSession),
  );

  return (
    <NhsPage
      originPage="edit-session"
      title={`Edit time and capacity for ${date.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${searchParams.date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditSessionConfirmed
        updatedSession={updatedSession}
        site={site}
        date={searchParams.date}
      />
    </NhsPage>
  );
};

export default Page;
