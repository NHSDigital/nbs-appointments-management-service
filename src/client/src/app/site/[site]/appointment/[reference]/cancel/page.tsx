import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchBooking,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import CancelAppointmentPage from './cancel-appointment-page';
import { notFound } from 'next/navigation';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { dateFormat, parseToUkDatetime } from '@services/timeService';

type PageProps = {
  params: Promise<{
    site: string;
    reference: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const props = await params;
  await assertPermission(props.site, 'booking:cancel');

  const [site, booking, clinicalServices] = await Promise.all([
    fetchSite(props.site),
    fetchBooking(props.reference, props.site),
    fetchClinicalServices(),
  ]);

  if (!booking || booking.status === 'Cancelled') {
    notFound();
  }

  const returnDate = parseToUkDatetime(booking.from).format(dateFormat);
  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${props.site}/view-availability/daily-appointments?date=${returnDate}&page=1`,
    text: 'Go back',
  };

  return (
    <NhsPage
      caption="Cancel appointment"
      title="Are you sure you want to cancel this appointment?"
      originPage="appointment-cancel"
      backLink={backLink}
      site={site}
    >
      <CancelAppointmentPage
        booking={booking}
        site={site.id}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
