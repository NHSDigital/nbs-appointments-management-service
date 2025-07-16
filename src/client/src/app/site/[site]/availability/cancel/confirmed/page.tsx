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
  searchParams?: Promise<{
    date: string;
    session: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { session, date } = { ...(await searchParams) };

  await assertPermission(siteFromPath, 'availability:setup');
  if (session === undefined || date === undefined) {
    notFound();
  }

  const [site, clinicalServices] = await Promise.all([
    fetchSite(siteFromPath),
    fetchClinicalServices(),
  ]);

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week/?date=${date}`,
    text: 'Back to week view',
  };

  return (
    <NhsPage
      title={`Cancelled session for ${parseToUkDatetime(date).format('DD MMMM YYYY')}`}
      caption={`${site.name}`}
      originPage="edit-session"
      backLink={backLink}
    >
      <CancellationConfirmed
        session={session}
        date={date}
        site={site.id}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
