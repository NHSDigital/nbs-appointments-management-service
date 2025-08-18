import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import AvailabilityTemplateWizard from './availability-template-wizard';
import fromServer from '@server/fromServer';

type PageProps = {
  searchParams?: Promise<{
    date: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date } = { ...(await searchParams) };

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const [site, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  await fromServer(assertPermission(site.id, 'availability:setup'));

  return (
    <NhsTransactionalPage originPage="create-availability-wizard">
      <AvailabilityTemplateWizard
        site={site}
        date={date}
        clinicalServices={clinicalServices}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
