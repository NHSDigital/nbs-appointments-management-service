import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import dayjs from 'dayjs';
import { notFound } from 'next/navigation';
import { EditSessionDecision } from './edit-session-decision';
import { dayStringFormat } from '@services/timeService';

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

  //TODO refactor!!
  const date = dayjs(searchParams.date, dayStringFormat);

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
        clinicalServices={clinicalServices}
      ></EditSessionDecision>
    </NhsPage>
  );
};

export default Page;
