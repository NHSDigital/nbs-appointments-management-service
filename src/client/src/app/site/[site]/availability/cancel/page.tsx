import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import ConfirmCancellation from './confirm-cancellation';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

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

  if (session === undefined || date === undefined) {
    notFound();
  }

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const [site, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week/edit-session?session=${session}&date=${date}`,
    text: 'Go back',
  };

  return (
    <NhsTransactionalPage
      title="Are you sure you want to cancel this session?"
      caption="Cancel session"
      originPage="edit-session"
      backLink={backLink}
    >
      <ConfirmCancellation
        date={date}
        session={session}
        site={site.id}
        clinicalServices={clinicalServices}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
