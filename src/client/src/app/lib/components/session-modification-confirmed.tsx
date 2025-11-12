import {
  ClinicalService,
  SessionSummary,
  SessionModificationAction,
} from '@types';
import { Card, InsetText } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { SessionSummaryTable } from '@components/session-summary-table';

type PageProps = {
  clinicalServices: ClinicalService[];
  siteId: string;
  date: string;
  modificationAction: SessionModificationAction;
  sessionSummary: SessionSummary;
  newlyOrphanedBookingsCount: number;
  bookingsCanceledWithoutDetails: number;
};

export const SessionModificationConfirmed = ({
  clinicalServices,
  siteId,
  date,
  modificationAction,
  sessionSummary,
  newlyOrphanedBookingsCount,
  bookingsCanceledWithoutDetails,
}: PageProps) => {
  const renderCommsSummary = () => {
    const commsSentForAllCancelledBookings =
      bookingsCanceledWithoutDetails === 0;

    if (commsSentForAllCancelledBookings) {
      return (
        <p>
          {newlyOrphanedBookingsCount} people have been sent a text message or
          email confirming their appointment has been cancelled.
        </p>
      );
    } else {
      return (
        <>
          <p>
            {newlyOrphanedBookingsCount - bookingsCanceledWithoutDetails} people
            have been sent a text message or email confirming their appointment
            has been cancelled.
          </p>
          <InsetText>
            <p>
              {bookingsCanceledWithoutDetails} people did not provide an email
              or mobile number, and have not been notified that their bookings
              were cancelled.
              <Link
                href={`/site/${siteId}/availability/edit/no-notifications?date=${date}&cancelledWithoutDetailsCount=${bookingsCanceledWithoutDetails}&page=1`}
                className="margin-top-bottom"
              >
                View the list of people who have no been notified.
              </Link>
            </p>
          </InsetText>
        </>
      );
    }
  };
  const renderCancellationSummary = () => {
    if (newlyOrphanedBookingsCount === 0) {
      return <p>This session has been cancelled.</p>;
    } else if (modificationAction === 'keep-appointments') {
      return (
        <p>
          This session has been cancelled. {newlyOrphanedBookingsCount} bookings
          have not been cancelled.
        </p>
      );
    } else {
      return (
        <p>
          This session has been cancelled and {newlyOrphanedBookingsCount}{' '}
          bookings have been cancelled.
        </p>
      );
    }
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={[sessionSummary]}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        showBooked={false}
      />

      {renderCancellationSummary()}

      {modificationAction === 'cancel-appointments' && (
        <>
          <Card
            title={String(newlyOrphanedBookingsCount)}
            description="Bookings have been cancelled"
            maxWidth={250}
          />
          {renderCommsSummary()}
        </>
      )}

      <Link
        href={`/site/${siteId}/view-availability/week?date=${date}`}
        className="margin-top-bottom"
      >
        View all bookings for this week
      </Link>
    </>
  );
};
