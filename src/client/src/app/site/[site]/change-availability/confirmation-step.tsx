'use client';
import { InjectedWizardProps } from '@components/wizard';
import Link from 'next/link';
import { useFormContext } from 'react-hook-form';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import { Heading } from 'nhsuk-react-components';

interface Props {
  site: string;
}

const ConfirmationStep = ({ site }: InjectedWizardProps & Props) => {
  const { getValues } = useFormContext<ChangeAvailabilityFormValues>();
  const { cancellationSummary, cancellationDecision } = getValues();

  if (!cancellationSummary)
    throw new Error("Couldn't load cancellation summary");

  const renderNextSteps = () => {
    return (
      <>
        <Heading headingLevel="h3">Next steps</Heading>
        <br />
        <ul>
          <Link href={`/site/${site}/create-availability/wizard`}>
            <li>Create availability</li>
          </Link>
          <Link href={`/site/${site}/view-availability`}>
            <li>Go back to view availability</li>
          </Link>
        </ul>
      </>
    );
  };

  const sessionWithoutBookingsCancellation = () => (
    <>
      <Heading headingLevel="h2">
        {`${cancellationSummary.cancelledSessionsCount} ${
          cancellationSummary.cancelledSessionsCount > 1
            ? 'sessions'
            : 'session'
        } cancelled`}
      </Heading>

      {renderNextSteps()}
    </>
  );

  const keepBookingsCancellation = () => (
    <>
      <Heading headingLevel="h2">
        {`${cancellationSummary.cancelledSessionsCount} ${
          cancellationSummary.cancelledSessionsCount > 1
            ? 'sessions'
            : 'session'
        } cancelled`}
      </Heading>

      <p>
        The bookings will remain in your appointments list. If you create new
        sessions they will automatically fill any matching appointment slots.
      </p>

      {renderNextSteps()}
    </>
  );

  const cancelBookingsAllNotifiedCancellation = () => (
    <>
      <Heading headingLevel="h2">
        {`${cancellationSummary.cancelledSessionsCount} ${
          cancellationSummary.cancelledSessionsCount > 1
            ? 'sessions'
            : 'session'
        } and 
        ${cancellationSummary.cancelledBookingsCount} 
        ${
          cancellationSummary.cancelledBookingsCount > 1
            ? 'bookings'
            : 'booking'
        } cancelled`}
      </Heading>

      <p>
        We have sent a text message or email to everyone we have contact details
        for to confirm the cancellation.
      </p>

      <br />
      <br />

      {renderNextSteps()}
    </>
  );

  const cancelBookingsNoOneNotifiedCancellation = () => (
    <>
      <Heading headingLevel="h2">
        {`${cancellationSummary.cancelledSessionsCount} ${
          cancellationSummary.cancelledSessionsCount > 1
            ? 'sessions'
            : 'session'
        } and 
        ${cancellationSummary.cancelledBookingsCount} 
        ${
          cancellationSummary.cancelledBookingsCount > 1
            ? 'bookings'
            : 'booking'
        } cancelled`}
      </Heading>

      <p>
        No one with a booking provided an email address or mobile number. If you
        can, you should contact people to tell them their booking is cancelled.
      </p>

      {/* TODO: link to be updated in APPT-1967 */}
      <Link href={`/site/${site}/change-availability`}>
        View the list of people whi have not been notified.
      </Link>

      <br />
      <br />

      {renderNextSteps()}
    </>
  );

  const cancelBookingsSomeNotNotifiedCancellation = () => (
    <>
      <Heading headingLevel="h2">
        {`${cancellationSummary.cancelledSessionsCount} ${
          cancellationSummary.cancelledSessionsCount > 1
            ? 'sessions'
            : 'session'
        } and 
        ${cancellationSummary.cancelledBookingsCount} 
        ${
          cancellationSummary.cancelledBookingsCount > 1
            ? 'bookings'
            : 'booking'
        } cancelled`}
      </Heading>

      <p>
        We have sent a text message or email to everyone we have contact details
        for to confirm the cancellation.
      </p>

      <Heading headingLevel="h3">
        {cancellationSummary.bookingsWithoutContactDetailsCount} people have not
        been notified
      </Heading>

      <p>
        These people did not provide an email address or mobile number. If you
        can, you should contact them to tell them their booking is cancelled.
      </p>
      {/* TODO: link to be updated in APPT-1967 */}
      <Link href={`/site/${site}/change-availability`}>
        View the list of people who have not been notified
      </Link>

      <br />
      <br />

      {renderNextSteps()}
    </>
  );

  const renderContent = () => {
    if (cancellationDecision == undefined) {
      return sessionWithoutBookingsCancellation();
    }

    if (cancellationDecision == 'keep-bookings') {
      return keepBookingsCancellation();
    }

    if (
      cancellationDecision == 'cancel-bookings' &&
      cancellationSummary.bookingsWithoutContactDetailsCount ==
        cancellationSummary.cancelledBookingsCount
    ) {
      return cancelBookingsAllNotifiedCancellation();
    }

    if (
      cancellationDecision == 'cancel-bookings' &&
      cancellationSummary.bookingsWithoutContactDetailsCount > 0 &&
      cancellationSummary.bookingsWithoutContactDetailsCount !=
        cancellationSummary.cancelledBookingsCount
    ) {
      return cancelBookingsSomeNotNotifiedCancellation();
    }

    return cancelBookingsNoOneNotifiedCancellation();
  };

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-two-thirds">{renderContent()}</div>
    </div>
  );
};

export default ConfirmationStep;
