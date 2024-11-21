'use client';
import {
  BackLink,
  Button,
  CheckBox,
  CheckBoxes,
  FormGroup,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import NhsHeading from '@components/nhs-heading';
import { clinicalServices } from '@types';

const SelectServicesStep = ({
  goToNextStep,
  goToLastStep,
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const { register, watch, trigger, formState, setValue, getValues } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors, isValid: allStepsAreValid, touchedFields } = formState;

  const servicesWatch = watch('session.services');

  const shouldSkipToSummaryStep =
    touchedFields.session?.services && allStepsAreValid;

  const onContinue = async () => {
    const formIsValid = await trigger(['session.services']);
    if (!formIsValid) {
      return;
    }

    if (shouldSkipToSummaryStep) {
      goToLastStep();
    } else {
      goToNextStep();
    }
  };

  const sessionType = getValues('sessionType');

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
        title="Add services to your session"
        caption={
          sessionType === 'single'
            ? 'Create single date session'
            : 'Create weekly session'
        }
      />

      <FormGroup error={errors.session?.services?.message}>
        <CheckBoxes>
          {clinicalServices.map(service => (
            <CheckBox
              id={`checkbox-${service.value.toLowerCase()}`}
              label={service.label}
              value={[service.value]}
              key={`checkbox-${service.value.toLowerCase()}`}
              {...register('session.services', {
                validate: value => {
                  if (value === undefined || value.length < 1) {
                    return 'Select a service';
                  }
                },
              })}
              onChange={() => {
                if ((servicesWatch ?? []).includes(service.value)) {
                  setValue(
                    'session.services',
                    clinicalServices
                      .filter(s => s.value !== service.value)
                      .map(_ => _.value),
                  );
                } else {
                  setValue('session.services', [
                    service.value,
                    ...(servicesWatch ?? []),
                  ]);
                }
                if (errors.session?.services) {
                  trigger('session.services');
                }
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
