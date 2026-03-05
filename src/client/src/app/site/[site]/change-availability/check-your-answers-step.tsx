'use client';
import {
  BackLink,
  Button,
  SmallSpinnerWithText,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import { InjectedWizardProps } from '@components/wizard';
import { useFormContext } from 'react-hook-form';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import { parseDateComponentsToUkDatetime } from '@services/timeService';

const CheckYourAnswersStep = ({
  goToPreviousStep,
  setCurrentStep,
  pendingSubmit,
}: InjectedWizardProps) => {
  const { getValues } = useFormContext<ChangeAvailabilityFormValues>();
  const { startDate, endDate, proposedCancellationSummary } = getValues();

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

  return (
    <>
      <BackLink
        onClick={() => goToPreviousStep()}
        renderingStrategy="client"
        text="Back"
      />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <NhsHeading title="Check your answers" />
          <SummaryList items={summary}></SummaryList>

          {pendingSubmit ? (
            <SmallSpinnerWithText text="Saving..." />
          ) : (
            <Button type="submit" styleType="warning">
              Cancel sessions
            </Button>
          )}
        </div>
      </div>
    </>
  );
};

export default CheckYourAnswersStep;
