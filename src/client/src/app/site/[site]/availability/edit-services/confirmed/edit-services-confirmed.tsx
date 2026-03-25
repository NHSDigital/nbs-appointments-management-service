import { AvailabilitySession, ClinicalService, Site } from '@types';
import Link from 'next/link';
import { BodyText, Card, InsetText } from 'nhsuk-react-components';
import { EditSessionConfirmedTableData } from '../../edit/confirmed/edit-session-confirmed-table-data';

type PageProps = {
  removedServicesSession: AvailabilitySession;
  clinicalServices: ClinicalService[];
  site: Site;
  date: string;
  hasBookings: boolean;
  servicesCount: number;
  chosenAction: string;
  newlyUnsupportedBookingsCount: number;
  cancelledWithDetailsCount: number;
  cancelledWithoutDetailsCount: number;
};

const EditServicesConfirmed = ({
  removedServicesSession,
  clinicalServices,
  site,
  date,
  chosenAction,
  hasBookings,
  servicesCount,
  newlyUnsupportedBookingsCount,
  cancelledWithDetailsCount,
  cancelledWithoutDetailsCount,
}: PageProps) => {
  return (
    <>
      <EditSessionConfirmedTableData
        updatedSession={removedServicesSession}
        clinicalServices={clinicalServices}
      />
      {hasBookings === false ? (
        <>
          <div className="margin-top-bottom">
            {servicesCount > 1 ? 'These services have' : 'The service has'} been
            removed.
          </div>
          <Link href={`/site/${site.id}/view-availability/week?date=${date}`}>
            View all bookings for this week
          </Link>
        </>
      ) : chosenAction === 'cancel-appointments' ? (
        <>
          <div className="margin-top-bottom">
            {servicesCount > 1 ? 'These services have' : 'The service has'} been
            removed and {newlyUnsupportedBookingsCount}{' '}
            {newlyUnsupportedBookingsCount > 1
              ? 'bookings have'
              : 'booking has'}{' '}
            been cancelled.
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
                  href={`/site/${site.id}/availability/edit-services/no-notifications?date=${date}&cancelledWithoutDetailsCount=${cancelledWithoutDetailsCount}&page=1`}
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
            {servicesCount > 1 ? 'These services have' : 'The service has'} been
            removed and {newlyUnsupportedBookingsCount}{' '}
            {newlyUnsupportedBookingsCount > 1
              ? 'bookings have'
              : 'booking has'}{' '}
            been cancelled.{' '}
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

export default EditServicesConfirmed;
