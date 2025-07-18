import {
  assertPermission,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import { AvailabilitySession } from '@types';
import NhsPage from '@components/nhs-page';
import { parseToUkDatetime } from '@services/timeService';
import EditServicesConfirmed from './edit-services-confirmed';
import { notFound } from 'next/navigation';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    removedServicesSession: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { date, removedServicesSession: serialisedSession } = {
    ...(await searchParams),
  };
  const { site: siteFromPath } = { ...(await params) };

  if (date === undefined || serialisedSession === undefined) {
    return notFound();
  }

  await assertPermission(siteFromPath, 'availability:setup');
  const [site, clinicalServices] = await Promise.all([
    fetchSite(siteFromPath),
    fetchClinicalServices(),
  ]);

  const parsedDate = parseToUkDatetime(date);

  const removedServicesSession: AvailabilitySession = JSON.parse(
    atob(serialisedSession),
  );

  return (
    <NhsPage
      originPage="edit-session"
      title={`Services removed for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${parsedDate.date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <EditServicesConfirmed
        removedServicesSession={removedServicesSession}
        site={site}
        date={date}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
