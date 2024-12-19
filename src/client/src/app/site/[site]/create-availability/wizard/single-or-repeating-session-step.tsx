/* eslint-disable react/jsx-props-no-spreading */
'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  FormGroup,
  Radio,
  RadioGroup,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { useFormContext } from 'react-hook-form';

const SingleOrRepeatingSessionStep = ({
  stepNumber,
  goToNextStep,
  goToLastStep,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const { register, reset, formState } =
    useFormContext<CreateAvailabilityFormValues>();
  const { isValid: allStepsAreValid, touchedFields } = formState;

  const shouldSkipToSummaryStep =
    touchedFields.session?.services && allStepsAreValid;

  const onContinue = async () => {
    if (shouldSkipToSummaryStep) {
      goToLastStep();
    } else {
      goToNextStep();
    }
  };

  const sessionType = { ...register('sessionType') };

  return (
    <>
      {stepNumber === 1 ? (
        <BackLink
          href={returnRouteUponCancellation ?? '/'}
          renderingStrategy="server"
          text="Go back"
        />
      ) : (
        <BackLink
          onClick={goToPreviousStep}
          renderingStrategy="client"
          text="Go back"
        />
      )}
      <NhsHeading
        title="What type of session do you want to create?"
        caption="Create availability"
      />
      <p>You can create weekly or single date sessions, to cover:</p>
      <ul>
        <li>Vaccinator availability</li>
        <li>Type of vaccine available</li>
      </ul>
      <br />
      <FormGroup>
        <RadioGroup>
          <Radio
            label="Weekly sessions"
            hint="Sessions that run at the same times every week"
            {...{
              ...sessionType,
              onChange: e => {
                reset({
                  days: [],
                  sessionType: 'repeating',
                  session: {
                    break: 'no',
                    services: [],
                  },
                });
                sessionType.onChange(e);
              },
            }}
            id="sessionType-repeating"
            value="repeating"
          />
          <Radio
            label="Single date session"
            hint="Sessions that run on one day and don't repeat"
            {...{
              ...sessionType,
              onChange: e => {
                reset({
                  days: [],
                  sessionType: 'single',
                  session: {
                    break: 'no',
                    services: [],
                  },
                });
                sessionType.onChange(e);
              },
            }}
            id="sessionType-single"
            value="single"
          />
        </RadioGroup>
      </FormGroup>

      <Button
        type="button"
        onClick={async () => {
          await onContinue();
        }}
      >
        Continue
      </Button>
    </>
  );
};

export default SingleOrRepeatingSessionStep;
