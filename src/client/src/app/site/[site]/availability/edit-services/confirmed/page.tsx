import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
import EditServicesConfirmed from './edit-services-confirmed';

type PageProps = {
  searchParams: {
    date: string;
    removedServicesSession: string;
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

  const removedServicesSession: AvailabilitySession = JSON.parse(
    atob(searchParams.removedServicesSession),
  );

  return (
    <NhsPage
      originPage="edit-session"
      title={`Services removed for ${date.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${searchParams.date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditServicesConfirmed
        removedServicesSession={removedServicesSession}
        site={site}
        date={searchParams.date}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
