import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchClinicalServices,
  fetchFeatureFlag,
  fetchSite,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import { EditSessionDecision } from './edit-session-decision';
import { parseToUkDatetime } from '@services/timeService';

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

  const [site, multipleServicesFlag, clinicalServices] = await Promise.all([
    fetchSite(params.site),
    fetchFeatureFlag('MultipleServices'),
    fetchClinicalServices(),
  ]);

  const date = parseToUkDatetime(searchParams.date);

  if (searchParams.session === undefined || searchParams.date === undefined) {
    notFound();
  }

  return (
    <NhsPage
      title={`Change availability for ${date.format('DD MMMM YYYY')}`}
      caption={site.name}
      site={site}
      backLink={{
        renderingStrategy: 'server',
        href: `/site/${params.site}/view-availability/week/?date=${searchParams.date}`,
        text: 'Go back',
      }}
      originPage="edit-session"
    >
      <EditSessionDecision
        site={site}
        sessionSummary={searchParams.session}
        date={searchParams.date}
        multipleServicesEnabled={multipleServicesFlag.enabled}
        clinicalServices={clinicalServices}
      ></EditSessionDecision>
    </NhsPage>
  );
};

export default Page;
