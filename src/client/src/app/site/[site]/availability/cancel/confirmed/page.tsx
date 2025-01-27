import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { notFound } from 'next/navigation';
import CancellationConfirmed from './cancellation-confirmed';
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
  const [site] = await Promise.all([
    fetchSite(params.site),
    assertPermission(params.site, 'availability:setup'),
  ]);

  if (searchParams.session === undefined || searchParams.date === undefined) {
    notFound();
  }

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/view-availability/week/?date=${searchParams.date}`,
    text: 'Back to week view',
  };

  return (
    <NhsPage
      title={`Cancelled session for ${dayjs(searchParams.date).format('DD MMMM YYYY')}`}
      caption={`${site.name}`}
      originPage="edit-session"
      backLink={backLink}
    >
      <CancellationConfirmed
        session={searchParams.session}
        date={searchParams.date}
        site={params.site}
      />
    </NhsPage>
  );
};

export default Page;
