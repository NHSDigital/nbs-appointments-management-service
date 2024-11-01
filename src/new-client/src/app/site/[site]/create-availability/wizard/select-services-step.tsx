'use client';
import {
  Button,
  CheckBox,
  CheckBoxes,
  FormGroup,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import NhsHeading from '@components/nhs-heading';

const SelectServicesStep = ({ goToNextStep }: InjectedWizardProps) => {
  const { register, watch, setError, formState } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const servicesWatch = watch('session.services');

  const onContinue = async () => {
    if ((servicesWatch ?? []).length < 1) {
      setError('session.services', {
        message: 'At least one service must be selected',
      });
      return;
    }

    goToNextStep();
  };

  // TODO: Decide where we're deriving this from
  const services = [{ label: 'RSV', value: 'RSV:Adult' }];

  return (
    <>
      <NhsHeading
        title="Add services to your session"
        caption="Create availability period"
      />

      <FormGroup error={errors.session?.services?.message}>
        <CheckBoxes>
          {services.map(service => (
            <CheckBox
              id={`checkbox-${service.value.toLowerCase()}`}
              label={service.label}
              value={[service.value]}
              key={`checkbox-${service.value.toLowerCase()}`}
              {...register('session.services')}
            />
          ))}
        </CheckBoxes>
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

export default SelectServicesStep;
