import { AvailabilitySession, ClinicalService, Site } from '@types';
import Link from 'next/link';
import { BodyText, Card, InsetText } from 'nhsuk-react-components';
import { EditSessionConfirmedTableData } from './edit-session-confirmed-table-data';

type PageProps = {
  updatedSession: AvailabilitySession;
  clinicalServices: ClinicalService[];
  site: Site;
  date: string;
  hasBookings: boolean;
  chosenAction: string;
  newlyUnsupportedBookingsCount: number;
  cancelledWithDetailsCount: number;
  cancelledWithoutDetailsCount: number;
  changeSessionUpliftedJourneyEnabled: boolean;
};

const EditSessionConfirmed = ({
  updatedSession,
  clinicalServices,
  site,
  date,
  chosenAction,
  hasBookings,
  newlyUnsupportedBookingsCount,
  cancelledWithDetailsCount,
  cancelledWithoutDetailsCount,
  changeSessionUpliftedJourneyEnabled,
}: PageProps) => {
  return (
    <>
      <EditSessionConfirmedTableData
        updatedSession={updatedSession}
        clinicalServices={clinicalServices}
      />

      {changeSessionUpliftedJourneyEnabled === false ? (
        <>
          <InsetText>
            <p>
              Some booked appointments may be affected by this change. If so,
              you'll need to cancel these appointments manually.
            </p>
          </InsetText>

          <Link
            href={`/site/${site.id}/view-availability/daily-appointments?date=${date}&page=1`}
          >
            Cancel appointments
          </Link>
        </>
      ) : hasBookings === false ? (
        <>
          <div className="margin-top-bottom">The session has been changed.</div>
          <Link href={`/site/${site.id}/view-availability/week?date=${date}`}>
            View all bookings for this week
          </Link>
        </>
      ) : chosenAction === 'cancel-appointments' ? (
        <>
          <div className="margin-top-bottom">
            This session has been updated and the new time and capacity has been
            saved.
          </div>
          <Card style={{ maxWidth: 250 }}>
            <Card.Heading>
              {newlyUnsupportedBookingsCount.toString()}
            </Card.Heading>
            <BodyText>
              {newlyUnsupportedBookingsCount > 1
                ? 'Bookings have been cancelled'
                : 'Booking has been cancelled'}
            </BodyText>
          </Card>
          {cancelledWithDetailsCount > 0 && (
            <div className="margin-top-bottom">
              {cancelledWithDetailsCount}{' '}
              {cancelledWithDetailsCount > 1 ? 'people' : 'person'} will be sent
              a text message or email confirming their appointment has been
              cancelled.
            </div>
          )}
          {cancelledWithoutDetailsCount > 0 && (
            <InsetText>
              <p>
                {cancelledWithoutDetailsCount}{' '}
                {cancelledWithoutDetailsCount > 1 ? 'people' : 'person'} did not
                provide an email or mobile number, and have not been notified
                that their booking has been cancelled.{' '}
                <Link
                  href={`/site/${site.id}/availability/edit/no-notifications?date=${date}&cancelledWithoutDetailsCount=${cancelledWithoutDetailsCount}&page=1`}
                >
                  View the list of people who have not been notified
                </Link>
                .
              </p>
            </InsetText>
          )}
          <Link href={`/site/${site.id}/view-availability/week?date=${date}`}>
            View all bookings for this week
          </Link>
        </>
      ) : (
        <>
          <div className="margin-top-bottom">
            This session has been updated and the new time and capacity has been
            saved.
          </div>
          {newlyUnsupportedBookingsCount > 0 && (
            <div className="margin-top-bottom">
              {newlyUnsupportedBookingsCount}{' '}
              {newlyUnsupportedBookingsCount > 1
                ? 'appointments have'
                : 'appointment has'}{' '}
              not been cancelled.
            </div>
          )}
          <Link href={`/site/${site.id}/view-availability/week?date=${date}`}>
            View all bookings for this week
          </Link>
        </>
      )}
    </>
  );
};

export default EditSessionConfirmed;
