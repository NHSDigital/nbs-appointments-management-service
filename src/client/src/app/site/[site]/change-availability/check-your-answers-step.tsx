'use client';
import {
  BackLink,
  SmallSpinnerWithText,
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
import { SummaryList, Button } from 'nhsuk-react-components';

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
  const {
    startDate,
    endDate,
    proposedCancellationSummary,
    cancellationDecision,
  } = getValues();

  if (!proposedCancellationSummary)
    throw new Error("Couldn't load proposed cancellation summary.");

  const startDayjs = parseDateComponentsToUkDatetime(startDate);
  const endDayjs = parseDateComponentsToUkDatetime(endDate);
  const isSameYear = startDayjs?.isSame(endDayjs, 'year');
  const formatedFullDateValue = isSameYear
    ? `${startDayjs?.format('D MMMM')} to ${endDayjs?.format('D MMMM YYYY')}`
    : `${startDayjs?.format('D MMMM YYYY')} to ${endDayjs?.format('D MMMM YYYY')}`;

  const cancellationDecisionSummary = () => {
    return cancellationDecision == 'keep-bookings'
      ? 'Keep bookings'
      : `Cancel ${proposedCancellationSummary.bookingCount} ${proposedCancellationSummary.bookingCount > 1 ? 'bookings' : 'booking'}`;
  };

  const getSummaryList = () => {
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

    if (
      proposedCancellationSummary.bookingCount > 0 &&
      proposedCancellationSummary.sessionCount > 0
    ) {
      summary.push({
        title: 'What you have chosen to do with the bookings',
        value: cancellationDecisionSummary(),
        action: {
          renderingStrategy: 'client',
          text: 'Change',
          onClick: () => {
            goToPreviousStep();
          },
        },
      });
    }

    return summary;
  };

  const onContinue = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (!startDayjs || !endDayjs) throw new Error('Invalid dates');

      const response = await cancelDateRange({
        site: site,
        from: startDayjs.format(RFC3339Format),
        to: endDayjs.format(RFC3339Format),
        cancelBookings: cancellationDecision == 'cancel-bookings',
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
          <SummaryList>
            {getSummaryList().map((item, index) => (
              <SummaryList.Row key={index}>
                <SummaryList.Key>{item.title}</SummaryList.Key>
                <SummaryList.Value>{item.value}</SummaryList.Value>
                {item.action && (
                  <SummaryList.Action
                    href={
                      item.action.renderingStrategy === 'server'
                        ? item.action.href
                        : '#'
                    }
                    onClick={e => {
                      if (item.action?.renderingStrategy === 'client') {
                        e.preventDefault();
                        item.action.onClick();
                      }
                    }}
                    visuallyHiddenText={item.title}
                  >
                    {item.action.text}
                  </SummaryList.Action>
                )}
              </SummaryList.Row>
            ))}
          </SummaryList>

          {pendingSubmit ? (
            <SmallSpinnerWithText text="Saving..." />
          ) : (
            <Button type="submit" warning onClick={onContinue}>
              {cancellationDecision == 'cancel-bookings'
                ? 'Cancel sessions and bookings'
                : 'Cancel sessions'}
            </Button>
          )}
        </div>
      </div>
    </>
  );
};

export default CheckYourAnswersStep;
