import { CancelDayResponse } from '@types';
import Link from 'next/link';

type PageProps = {
  site: string;
  date: string;
  dayCancellationSummary: CancelDayResponse;
};

//Should cancelled appointments include those cancelled prior to those already cancelled before ?

const CancellationConfirmed = ({
  site,
  date,
  dayCancellationSummary,
}: PageProps) => {
  return (
    <>
      <p className="nhsuk-body">
        {dayCancellationSummary.cancelledBookingCount} appointments
        {'  '}
        have been cancelled.&nbsp;
        {dayCancellationSummary.cancelledBookingCount -
          dayCancellationSummary.cancelledBookingCount}{' '}
        people will be sent a text message or email confirming their appointment
        has been cancelled.
      </p>
      <p className="nhsuk-body">
        {dayCancellationSummary.bookingsWithoutContactDetails} people did not
        provide contact details so they will not receive a notification.
      </p>
      <div className="nhsuk-u-margin-top-4">
        <Link
          href={`/site/${site}/cancel-day/cancelled-appointments?date=${date}&page=1`}
          className="nhsuk-link nhsuk-u-display-block nhsuk-u-margin-bottom-2"
        >
          View bookings without contact details
        </Link>
      </div>
      <div className="nhsuk-u-margin-top-4">
        <Link
          href={`/site/${site}/view-availability/week?date=${date}`}
          className="nhsuk-link nhsuk-u-display-block"
        >
          View all bookings for this week
        </Link>
      </div>
    </>
  );
};

export default CancellationConfirmed;
