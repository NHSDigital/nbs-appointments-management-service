import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import AvailabilityTemplateWizard from './availability-template-wizard';

type PageProps = {
  searchParams: Promise<{
    date: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const [search, props] = await Promise.all([searchParams, params]);
  await assertPermission(props.site, 'availability:setup');

  const [site, clinicalServices] = await Promise.all([
    fetchSite(props.site),
    fetchClinicalServices(),
  ]);

  await assertPermission(site.id, 'availability:setup');

  return (
    <NhsTransactionalPage originPage="create-availability-wizard">
      <AvailabilityTemplateWizard
        site={site}
        date={search.date}
        clinicalServices={clinicalServices}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
