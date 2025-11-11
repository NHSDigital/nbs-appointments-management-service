import { SessionBookingsContactDetailsPage } from '@components/session-bookings-contact-details';
import { Booking, ClinicalService } from '@types';

type Props = {
  bookings: Booking[];
  site: string;
  clinicalServices: ClinicalService[];
  cancelledWithoutDetailsCount: number;
};

export const NoNotificationsPage = ({
  bookings,
  site,
  clinicalServices,
  cancelledWithoutDetailsCount,
}: Props) => {
  return (
    <>
      {cancelledWithoutDetailsCount > 0 && (
        <p className="no-print">
          {cancelledWithoutDetailsCount}{' '}
          {cancelledWithoutDetailsCount > 1 ? 'people' : 'person'} did not get a
          cancellation notification as they had no contact details on their
          booking.
        </p>
      )}
      <SessionBookingsContactDetailsPage
        bookings={bookings}
        site={site}
        displayAction={false}
        clinicalServices={clinicalServices}
      />
    </>
  );
};
