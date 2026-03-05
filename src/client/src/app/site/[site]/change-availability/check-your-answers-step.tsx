'use client';
import {
  BackLink,
  Button,
  SmallSpinnerWithText,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { Heading } from 'nhsuk-react-components';
import { InjectedWizardProps } from '@components/wizard';
import { useFormContext } from 'react-hook-form';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import {
  parseDateComponentsToUkDatetime,
  RFC3339Format,
} from '@services/timeService';
import { cancelDateRange } from '@services/appointmentsService';
import { useState } from 'react';

interface Props {
  site: string;
}

const CheckYourAnswersStep = ({
  site,
  goToPreviousStep,
  setCurrentStep,
  goToNextStep,
  pendingSubmit,
}: InjectedWizardProps & Props) => {
  const [error, setError] = useState<Error | null>(null);

  if (error) {
    throw error;
  }

  const { getValues, setValue } =
    useFormContext<ChangeAvailabilityFormValues>();
  const { startDate, endDate, proposedCancellationSummary } = getValues();

  const cancelBookings = false;
  const startDayjs = parseDateComponentsToUkDatetime(startDate);
  const endDayjs = parseDateComponentsToUkDatetime(endDate);
  const isSameYear = startDayjs?.isSame(endDayjs, 'year');
  const formatedFullDateValue = isSameYear
    ? `${startDayjs?.format('D MMMM')} to ${endDayjs?.format('D MMMM YYYY')}`
    : `${startDayjs?.format('D MMMM YYYY')} to ${endDayjs?.format('D MMMM YYYY')}`;

  const summary: SummaryListItem[] = [
    {
      title: 'Dates',
      value: formatedFullDateValue,
      action: {
        renderingStrategy: 'client',
        text: 'Change',
        onClick: () => {
          setCurrentStep(2);
        },
      },
    },
    {
      title: 'Number of sessions',
      value: `${proposedCancellationSummary?.sessionCount}`,
    },
  ];

  const onContinue = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (!startDayjs || !endDayjs) throw new Error('Invalid dates');

      const response = await cancelDateRange({
        site: site,
        from: startDayjs.format(RFC3339Format),
        to: endDayjs.format(RFC3339Format),
        cancelBookings: cancelBookings,
      });

      if (!response.success) return;

      setValue('cancellationSummary', {
        cancelledSessionsCount: response.data.cancelledSessionsCount,
        cancelledBookingsCount: response.data.cancelledBookingsCount,
        bookingsWithoutContactDetailsCount:
          response.data.bookingsWithoutContactDetailsCount,
      });

      goToNextStep();
    } catch (err) {
      setError(err as Error);
    }
  };

  return (
    <>
      <BackLink
        onClick={() => goToPreviousStep()}
        renderingStrategy="client"
        text="Back"
      />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <Heading headingLevel="h2">Check your answers</Heading>
          <SummaryList items={summary}></SummaryList>

          {pendingSubmit ? (
            <SmallSpinnerWithText text="Saving..." />
          ) : (
            <Button type="submit" styleType="warning" onClick={onContinue}>
              Cancel sessions
            </Button>
          )}
        </div>
      </div>
    </>
  );
};

export default CheckYourAnswersStep;
