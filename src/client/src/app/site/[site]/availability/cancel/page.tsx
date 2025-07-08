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
    href: `/site/${site.id}/view-availability/week/edit-session?session=${search.session}&date=${search.date}`,
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
        date={search.date}
        session={search.session}
        site={props.site}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
