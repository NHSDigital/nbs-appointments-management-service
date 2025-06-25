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

  const [site, clinicalServices] = await Promise.all([
    fetchSite(params.site),
    fetchClinicalServices(),
  ]);

  const date = parseToUkDatetime(searchParams.date);
  const sessionSummary: SessionSummary = JSON.parse(atob(searchParams.session));

  return (
    <NhsPage
      title={`Remove services for ${date.format('DD MMMM YYYY')}`}
      caption={'Remove services'}
      originPage="edit-session"
      backLink={{
        href: `/site/${site.id}/view-availability/week/edit-session?session=${searchParams.session}&date=${searchParams.date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <EditServicesForm
        date={searchParams.date}
        site={site}
        existingSession={sessionSummary}
        clinicalServices={clinicalServices}
      ></EditServicesForm>
    </NhsPage>
  );
};

export default Page;
