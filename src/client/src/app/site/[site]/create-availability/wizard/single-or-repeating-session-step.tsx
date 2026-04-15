/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { BackLink, FormGroup } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { useFormContext } from 'react-hook-form';
import { Heading, Button, Radios } from 'nhsuk-react-components';

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
          href={returnRouteUponCancellation ?? '/sites'}
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
      <Heading headingLevel="h2">
        <span className="nhsuk-caption-l">Create availability</span>
        What type of session do you want to create?
      </Heading>
      <p>You can create weekly or single date sessions, to cover:</p>
      <ul>
        <li>Vaccinator availability</li>
        <li>Type of vaccine available</li>
      </ul>
      <br />
      <FormGroup>
        <Radios>
          <Radios.Item
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
          >
            Weekly sessions
          </Radios.Item>
          <Radios.Item
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
          >
            Single date session
          </Radios.Item>
        </Radios>
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
