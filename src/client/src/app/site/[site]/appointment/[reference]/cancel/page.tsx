import {
  assertPermission,
  fetchBooking,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import CancelAppointmentPage from './cancel-appointment-page';
import { notFound } from 'next/navigation';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { RFC3339Format, parseToUkDatetime } from '@services/timeService';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

type PageProps = {
  params: Promise<{
    site: string;
    reference: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath, reference } = { ...(await params) };

  await fromServer(assertPermission(siteFromPath, 'booking:cancel'));

  const [site, booking, clinicalServices] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchBooking(reference, siteFromPath)),
    fromServer(fetchClinicalServices()),
  ]);

  if (!booking || booking.status === 'Cancelled') {
    notFound();
  }

  const returnDate = parseToUkDatetime(booking.from).format(RFC3339Format);
  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/daily-appointments?date=${returnDate}&page=1`,
    text: 'Go back',
  };

  return (
    <NhsTransactionalPage
      caption="Cancel appointment"
      title="Are you sure you want to cancel this appointment?"
      originPage="appointment-cancel"
      backLink={backLink}
    >
      <CancelAppointmentPage
        booking={booking}
        site={site.id}
        clinicalServices={clinicalServices}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
