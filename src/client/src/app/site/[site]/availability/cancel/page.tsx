import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import ConfirmCancellation from './confirm-cancellation';
import NhsPage from '@components/nhs-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

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

  if (searchParams.session === undefined || searchParams.date === undefined) {
    notFound();
  }

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week/edit-session?session=${searchParams.session}&date=${searchParams.date}`,
    text: 'Go back',
  };

  return (
    <NhsPage
      title="Are you sure you want to cancel this session?"
      caption="Cancel session"
      originPage="edit-session"
      backLink={backLink}
    >
      <ConfirmCancellation
        date={searchParams.date}
        session={searchParams.session}
        site={params.site}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
