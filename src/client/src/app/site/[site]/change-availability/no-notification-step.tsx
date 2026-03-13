'use client';
import { BackLink } from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import PrintPageButton from '@components/print-page-button';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import {
  parseDateComponentsToUkDatetime,
  dateTimeFormat,
} from '@services/timeService';
import { useState, useEffect, useMemo } from 'react';
import { FetchBookingsRequest, Booking, ClinicalService } from '@types';
import fromServer from '@server/fromServer';
import {
  fetchBookings,
  fetchClinicalServices,
} from '@services/appointmentsService';
import { SessionBookingsContactDetailsPage } from '@components/session-bookings-contact-details';
import { Heading, InsetText } from 'nhsuk-react-components';

interface Props {
  site: string;
}

const NoNotificationStep = ({ site }: InjectedWizardProps & Props) => {
  const [error, setError] = useState<Error | null>(null);

  if (error) {
    throw error;
  }
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [clinicalServices, setClinicalServices] = useState<ClinicalService[]>(
    [],
  );
  const [isLoading, setIsLoading] = useState(true);
  const { getValues } = useFormContext<ChangeAvailabilityFormValues>();
  const { startDate, endDate, cancellationSummary } = getValues();

  const startDateUkDateTime = useMemo(
    () => parseDateComponentsToUkDatetime(startDate),
    [startDate],
  );

  const endDateUkDateTime = useMemo(
    () => parseDateComponentsToUkDatetime(endDate),
    [endDate],
  );
  const isSameYear = startDateUkDateTime?.isSame(endDateUkDateTime, 'year');
  const formatedFullDateValue = isSameYear
    ? `${startDateUkDateTime?.format('D MMMM')} to ${endDateUkDateTime?.format('D MMMM YYYY')}`
    : `${startDateUkDateTime?.format('D MMMM YYYY')} to ${endDateUkDateTime?.format('D MMMM YYYY')}`;

  if (!startDateUkDateTime || !endDateUkDateTime)
    throw new Error("Couldn't parse dates.");

  if (!cancellationSummary)
    throw new Error("Couldn't load cancellation summary");

  const cancelledWithoutDetailsCount =
    cancellationSummary.bookingsWithoutContactDetailsCount;

  useEffect(() => {
    const loadCancelledBookings = async () => {
      try {
        setIsLoading(true);
        const fetchBookingsRequest: FetchBookingsRequest = {
          from: startDateUkDateTime.format(dateTimeFormat),
          to: endDateUkDateTime.endOf('day').format(dateTimeFormat),
          site: site,
        };

        const [cancelledBookingsResult, clinicalServicesResult] =
          await Promise.all([
            fromServer(fetchBookings(fetchBookingsRequest, ['Cancelled'])),
            fromServer(fetchClinicalServices()),
          ]);

        setBookings(cancelledBookingsResult);
        setClinicalServices(clinicalServicesResult);
      } catch (err) {
        setError(err as Error);
      } finally {
        setIsLoading(false);
      }
    };

    loadCancelledBookings();
  }, [startDateUkDateTime, endDateUkDateTime, site]);

  return (
    <>
      <BackLink
        href={`/site/${site}/view-availability`}
        renderingStrategy="server"
        text="Back to view availability"
      />
      {isLoading ? (
        <p>Loading booking details...</p>
      ) : (
        <>
          <Heading headingLevel="h2">
            People who did not receive a notification
          </Heading>
          <span className="no-print">
            <InsetText>
              Print this page to keep a record of these people. You will not be
              able to see this list again after you leave this screen.
            </InsetText>
            <PrintPageButton />

            {cancelledWithoutDetailsCount > 0 && (
              <Heading headingLevel="h3">
                {cancelledWithoutDetailsCount}
                {cancelledWithoutDetailsCount > 1 ? ' people' : ' person'} with
                appointments from {formatedFullDateValue}
              </Heading>
            )}

            <p>
              These people have no email address or mobile number on their
              booking. They did not receive a notification.
            </p>
          </span>

          <SessionBookingsContactDetailsPage
            bookings={bookings}
            site={site}
            displayAction={false}
            clinicalServices={clinicalServices}
          />
        </>
      )}
    </>
  );
};

export default NoNotificationStep;
