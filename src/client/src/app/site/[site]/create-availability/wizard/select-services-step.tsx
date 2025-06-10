'use client';
import {
  BackLink,
  Button,
  CheckBox,
  CheckBoxes,
  CheckBoxOrAll,
  FormGroup,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import NhsHeading from '@components/nhs-heading';
import { ClinicalService } from '@types';

type SelectServicesStepProps = {
  clinicalServices: ClinicalService[];
};

const SelectServicesStep = ({
  goToNextStep,
  goToLastStep,
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
  clinicalServices,
}: InjectedWizardProps & SelectServicesStepProps) => {
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
          {clinicalServices.map(clinicalService => (
            <CheckBox
              id={`checkbox-${clinicalService.value}`}
              label={clinicalService.label}
              value={clinicalService.value}
              key={`checkbox-${clinicalService.value}`}
              {...register('session.services', {
                validate: value => {
                  if (value === undefined || value.length < 1) {
                    return 'Select a service';
                  }
                },
              })}
              onChange={() => {
                if ((servicesWatch ?? []).includes(clinicalService.value)) {
                  setValue(
                    'session.services',
                    servicesWatch.filter(d => d !== clinicalService.value),
                  );
                } else {
                  setValue('session.services', [
                    clinicalService.value,
                    ...(servicesWatch ?? []),
                  ]);
                }
                if (errors.session?.services) {
                  trigger('session.services');
                }
              }}
            />
          ))}
          {clinicalServices.length > 1 && (
            <>
              <CheckBoxOrAll />
              <CheckBox
                label={'Select all'}
                value={clinicalServices.map(_ => _.value)}
                id={'allServices'}
                checked={servicesWatch?.length == clinicalServices.length}
                onChange={() => {
                  if (servicesWatch?.length == clinicalServices.length) {
                    setValue('session.services', []);
                  } else {
                    setValue(
                      'session.services',
                      clinicalServices.map(_ => _.value),
                    );
                  }
                  if (errors.session?.services) {
                    trigger('session.services');
                  }
                }}
              />
            </>
          )}
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
