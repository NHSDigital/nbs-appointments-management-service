import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { SessionSummary } from '@types';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
import EditServicesForm from './edit-services-form';

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

  const [site, clinicalServices] = await Promise.all([
    fetchSite(props.site),
    fetchClinicalServices(),
  ]);

  await assertPermission(props.site, 'availability:setup');

  const date = parseToUkDatetime(search.date);
  const sessionSummary: SessionSummary = JSON.parse(atob(search.session));

  return (
    <NhsPage
      title={`Remove services for ${date.format('DD MMMM YYYY')}`}
      caption={'Remove services'}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/view-availability/week/edit-session?session=${search.session}&date=${search.date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <EditServicesForm
        date={search.date}
        site={site}
        existingSession={sessionSummary}
        clinicalServices={clinicalServices}
      ></EditServicesForm>
    </NhsPage>
  );
};

export default Page;
