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

const SERVICE_GROUPS_CONFIG = [
  {
    category: 'flu',
    title: 'Flu services',
    itemOrder: ['FLU:2_3', 'FLU:18_64', 'FLU:65+'],
  },
  {
    category: 'COVID-19',
    title: 'COVID-19 services',
    itemOrder: ['COVID:5_11', 'COVID:12_17', 'COVID:18+'],
  },
  {
    category: 'COVID-19 and flu',
    title: 'Flu and COVID-19 co-admin services',
    itemOrder: ['COVID_FLU:18_64', 'COVID_FLU:65+'],
  },
  {
    category: 'RSV',
    title: 'RSV services',
    itemOrder: ['RSV:Adult'],
  },
  {
    category: 'RSV and COVID-19 co-admin',
    title: 'RSV and COVID-19 co-admin services',
    itemOrder: ['RSV_COVID:18+'],
  },
];

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

      <p className="nhsuk-body">
        Co-admin appointments can only be booked by people eligible for both
        vaccinations.
      </p>

      <FormGroup error={errors.session?.services?.message}>
        {SERVICE_GROUPS_CONFIG.map(group => {
          const servicesInGroup = group.itemOrder
            .map(val => clinicalServices.find(s => s.value === val))
            .filter((s): s is ClinicalService => !!s);

          if (servicesInGroup.length === 0) return null;

          return (
            <fieldset
              key={group.category}
              className="nhsuk-fieldset app-checkbox-group"
              style={{ marginBottom: '32px' }}
            >
              <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
                <span className="nhsuk-u-visually-hidden">Add </span>
                {group.title}
                <span className="nhsuk-u-visually-hidden">
                  {' '}
                  to your session
                </span>
              </legend>

              <CheckBoxes>
                {servicesInGroup.map(service => (
                  <CheckBox
                    id={`checkbox-${service.value}`}
                    label={service.serviceType}
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
