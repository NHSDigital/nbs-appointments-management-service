'use client';
import {
  Button,
  CheckBox,
  CheckBoxes,
  FormGroup,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import {
  CreateAvailabilityFormValues,
  services,
} from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import NhsHeading from '@components/nhs-heading';

const SelectServicesStep = ({ goToNextStep }: InjectedWizardProps) => {
  const { register, watch, trigger, formState, setValue } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const servicesWatch = watch('session.services');

  const onContinue = async () => {
    const formIsValid = await trigger(['session.services']);
    if (!formIsValid) {
      return;
    }

    goToNextStep();
  };

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
              {...register('session.services', {
                validate: value => {
                  if (value === undefined || value.length < 1) {
                    return 'At least one service must be selected';
                  }
                },
              })}
              onChange={() => {
                if ((servicesWatch ?? []).includes(service.value)) {
                  setValue(
                    'session.services',
                    services
                      .filter(s => s.value !== service.value)
                      .map(_ => _.value),
                  );
                } else {
                  setValue('session.services', [
                    service.value,
                    ...(servicesWatch ?? []),
                  ]);
                }
                trigger('session.services');
              }}
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
