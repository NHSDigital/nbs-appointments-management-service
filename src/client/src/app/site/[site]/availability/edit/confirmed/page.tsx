import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
import EditSessionConfirmed from './edit-session-confirmed';

type PageProps = {
  searchParams: Promise<{
    date: string;
    updatedSession: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const [search, props] = await Promise.all([searchParams, params]);

  await assertPermission(props.site, 'availability:setup');
  const [site, clinicalServices] = await Promise.all([
    fetchSite(props.site),
    fetchClinicalServices(),
  ]);

  const date = parseToUkDatetime(search.date);

  const updatedSession: AvailabilitySession = JSON.parse(
    atob(search.updatedSession),
  );

  return (
    <NhsPage
      originPage="edit-session"
      title={`Edit time and capacity for ${date.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${search.date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditSessionConfirmed
        updatedSession={updatedSession}
        site={site}
        date={search.date}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
