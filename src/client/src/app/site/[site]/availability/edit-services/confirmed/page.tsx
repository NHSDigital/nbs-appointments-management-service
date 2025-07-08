import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { AvailabilitySession, Site } from '@types';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
import EditServicesConfirmed from './edit-services-confirmed';

type PageProps = {
  searchParams: Promise<{
    date: string;
    removedServicesSession: string;
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

  const removedServicesSession: AvailabilitySession = JSON.parse(
    atob(search.removedServicesSession),
  );

  return (
    <NhsPage
      originPage="edit-session"
      title={`Services removed for ${date.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${search.date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditServicesConfirmed
        removedServicesSession={
          new Promise<AvailabilitySession>(() => removedServicesSession)
        }
        site={new Promise<Site>(() => site)}
        date={new Promise<string>(() => search.date)}
        clinicalServices={fetchClinicalServices()}
      />
    </NhsPage>
  );
};

export default Page;
