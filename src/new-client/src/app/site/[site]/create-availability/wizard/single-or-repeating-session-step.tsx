/* eslint-disable react/jsx-props-no-spreading */
'use client';
import NhsHeading from '@components/nhs-heading';
import {
  Button,
  FormGroup,
  Radio,
  RadioGroup,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { useFormContext } from 'react-hook-form';

const SingleOrRepeatingSessionStep = ({
  goToNextStep,
}: InjectedWizardProps) => {
  const { register } = useFormContext<CreateAvailabilityFormValues>();

  const onContinue = async () => {
    goToNextStep();
  };

  return (
    <>
      <NhsHeading
        title="What type of session do you want to create?"
        caption="Create availability period"
      />
      <FormGroup>
        <RadioGroup>
          <Radio
            label="Repeat session"
            hint="Create sessions that repeat on a weekly basis"
            {...register('sessionType')}
            id="sessionType-repeating"
            value="repeating"
          />
          <Radio
            label="Single session"
            hint="Create a session on a single date"
            {...register('sessionType')}
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
