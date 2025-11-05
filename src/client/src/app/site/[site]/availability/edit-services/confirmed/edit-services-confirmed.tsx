import { Table, InsetText } from '@components/nhsuk-frontend';
import { AvailabilitySession, ClinicalService, Site } from '@types';
import { Card } from '@nhsuk-frontend-components';
import Link from 'next/link';

type PageProps = {
  removedServicesSession: AvailabilitySession;
  clinicalServices: ClinicalService[];
  site: Site;
  date: string;
  hasBookings: boolean;
  servicesCount: number;
  chosenAction: string;
  unsupportedBookingsCount: number;
  cancelledWithDetailsCount: number;
  cancelledWithoutDetailsCount: number;
  changeSessionUpliftedJourneyEnabled: boolean;
};

const EditServicesConfirmed = ({
  removedServicesSession,
  clinicalServices,
  site,
  date,
  chosenAction,
  hasBookings,
  servicesCount,
  unsupportedBookingsCount,
  cancelledWithDetailsCount,
  cancelledWithoutDetailsCount,
  changeSessionUpliftedJourneyEnabled,
}: PageProps) => {
  return (
    <>
      <Table
        headers={['Time', 'Services']}
        rows={[
          [
            <strong key={`session-0-start-and-end-time`}>
              {`${removedServicesSession.from} - ${removedServicesSession.until}`}
            </strong>,
            <>
              {removedServicesSession.services.map((service, serviceIndex) => {
                return (
                  <span key={`service-name-${serviceIndex}`}>
                    {clinicalServices.find(c => c.value === service)?.label ??
                      service}
                    <br />
                  </span>
                );
              })}
            </>,
          ],
        ]}
      />
      {changeSessionUpliftedJourneyEnabled === false ? (
        <InsetText>
          <p>
            Some booked appointments may be affected by this change. If so,
            you'll need to cancel these appointments manually.
          </p>
          <Link
            href={`/site/${site.id}/view-availability/daily-appointments?date=${date}&page=1&tab=2`}
          >
            Cancel appointments
          </Link>
        </InsetText>
      ) : hasBookings === false ? (
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
            removed and {unsupportedBookingsCount}{' '}
            {unsupportedBookingsCount > 1 ? 'bookings have' : 'booking has'}{' '}
            been cancelled.
          </div>
          {unsupportedBookingsCount > 1 ? (
            <Card
              title={unsupportedBookingsCount.toString()}
              description="Bookings have been cancelled"
              maxWidth={250}
            />
          ) : (
            <Card
              title={unsupportedBookingsCount.toString()}
              description="Booking has been cancelled"
              maxWidth={250}
            />
          )}
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
            {servicesCount > 1 ? 'These services have' : 'The service has'} been
            removed and {unsupportedBookingsCount}{' '}
            {unsupportedBookingsCount > 1 ? 'bookings have' : 'booking has'}{' '}
            been cancelled.{' '}
          </div>
          {unsupportedBookingsCount > 0 && (
            <div className="margin-top-bottom">
              {unsupportedBookingsCount}{' '}
              {unsupportedBookingsCount > 1
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
