import { SessionBookingsContactDetailsPage } from '@components/session-bookings-contact-details';
import { Booking, ClinicalService } from '@types';

type Props = {
  bookings: Booking[];
  site: string;
  clinicalServices: ClinicalService[];
};

export const NoNotificationsPage = ({
  bookings,
  site,
  clinicalServices,
}: Props) => {
  return (
    <SessionBookingsContactDetailsPage
      bookings={bookings}
      site={site}
      displayAction={false}
      clinicalServices={clinicalServices}
    />
  );
};
