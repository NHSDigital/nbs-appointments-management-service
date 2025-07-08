import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import CancellationConfirmed from './cancellation-confirmed';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { parseToUkDatetime } from '@services/timeService';

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

  await assertPermission(props.site, 'availability:setup');

  const [site, clinicalServices] = await Promise.all([
    fetchSite(props.site),
    fetchClinicalServices(),
  ]);

  if (search.session === undefined || search.date === undefined) {
    notFound();
  }

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${props.site}/view-availability/week/?date=${search.date}`,
    text: 'Back to week view',
  };

  return (
    <NhsPage
      title={`Cancelled session for ${parseToUkDatetime(search.date).format('DD MMMM YYYY')}`}
      caption={`${site.name}`}
      originPage="edit-session"
      backLink={backLink}
    >
      <CancellationConfirmed
        session={search.session}
        date={search.date}
        site={props.site}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
