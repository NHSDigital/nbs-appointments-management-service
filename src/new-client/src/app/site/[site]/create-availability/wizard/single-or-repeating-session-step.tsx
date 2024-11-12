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
        />
      ) : (
        <BackLink onClick={goToPreviousStep} renderingStrategy="client" />
      )}
      <NhsHeading
        title="What type of session do you want to create?"
        caption="Create availability period"
      />
      <FormGroup>
        <RadioGroup>
          <Radio
            label="Repeat session"
            hint="Create sessions that repeat on a weekly basis"
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
            label="Single session"
            hint="Create a session on a single date"
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
