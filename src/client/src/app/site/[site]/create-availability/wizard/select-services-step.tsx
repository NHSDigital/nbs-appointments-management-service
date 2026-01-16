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
import { ClinicalService } from '@types';

type SelectServicesStepProps = {
  clinicalServices: ClinicalService[];
};

const SERVICE_TYPE_TITLES: Record<string, string> = {
  flu: 'Flu services',
  'COVID-19': 'COVID-19 services',
  'COVID-19 and flu': 'Flu and COVID-19 co-admin services',
  RSV: 'RSV services',
  'RSV and COVID-19': 'RSV and COVID-19 co-admin services',
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
  const groupedServices = clinicalServices.reduce(
    (acc, service) => {
      const group = service.serviceType;
      if (!acc[group]) {
        acc[group] = [];
      }
      acc[group].push(service);
      return acc;
    },
    {} as Record<string, ClinicalService[]>,
  );

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

      <p className="nhsuk-body">
        Co-admin appointments can only be booked by people eligible for both
        vaccinations.
      </p>

      <FormGroup error={errors.session?.services?.message}>
        {Object.entries(groupedServices).map(([serviceType, services]) => {
          const groupTitle = SERVICE_TYPE_TITLES[serviceType] || serviceType;

          return (
            <fieldset
              key={serviceType}
              className="nhsuk-fieldset app-checkbox-group"
              style={{ marginBottom: '32px' }}
            >
              <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
                <span className="nhsuk-u-visually-hidden">Add </span>
                {groupTitle}
                <span className="nhsuk-u-visually-hidden">
                  {' '}
                  to your session
                </span>
              </legend>

              <CheckBoxes>
                {services.map(service => (
                  <CheckBox
                    id={`checkbox-${service.value}`}
                    label={service.label.replace('-', ' to ')}
                    value={service.value}
                    key={service.value}
                    {...register('session.services', {
                      validate: val =>
                        val?.length < 1 ? 'Select a service' : true,
                    })}
                    onChange={() => {
                      const current = servicesWatch ?? [];
                      const next = current.includes(service.value)
                        ? current.filter(v => v !== service.value)
                        : [...current, service.value];

                      setValue('session.services', next);
                      if (errors.session?.services) trigger('session.services');
                    }}
                  />
                ))}
              </CheckBoxes>
            </fieldset>
          );
        })}
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
