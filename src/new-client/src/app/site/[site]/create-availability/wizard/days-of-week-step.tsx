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
import { daysOfTheWeek } from '@types';

const DaysOfWeekStep = ({
  goToNextStep,
  goToLastStep,
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const { register, setValue, watch, formState, trigger } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors, isValid: allStepsAreValid, touchedFields } = formState;

  const daysWatch = watch('days');

  const shouldSkipToSummaryStep =
    touchedFields.session?.services && allStepsAreValid;

  const onContinue = async () => {
    const formIsValid = await trigger(['days']);
    if (!formIsValid) {
      return;
    }

    if (shouldSkipToSummaryStep) {
      goToLastStep();
    } else {
      goToNextStep();
    }
  };

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
        title="Select days to add to your weekly session"
        caption="Create weekly session"
      />

      <p>You can create multiple weekly sessions, to cover:</p>
      <ul>
        <li>Vaccinator availability</li>
        <li>Type of vaccine available</li>
      </ul>
      <br />

      <FormGroup error={errors.days?.message}>
        <CheckBoxes>
          {daysOfTheWeek.map(dayOfWeek => (
            <CheckBox
              id={`checkbox-${dayOfWeek.toLowerCase()}`}
              label={dayOfWeek}
              value={dayOfWeek}
              key={`checkbox-${dayOfWeek.toLowerCase()}`}
              {...register('days', {
                validate: value => {
                  if (value === undefined || value.length < 1) {
                    return 'Services must run on at least one day';
                  }
                },
              })}
              onChange={() => {
                if ((daysWatch ?? []).includes(dayOfWeek)) {
                  setValue(
                    'days',
                    daysWatch.filter(d => d !== dayOfWeek),
                  );
                } else {
                  setValue('days', [dayOfWeek, ...(daysWatch ?? [])]);
                }

                if (errors.days) {
                  trigger('days');
                }
              }}
            />
          ))}
          <CheckBoxOrAll />
          <CheckBox
            label={'Select all days'}
            value={daysOfTheWeek}
            checked={daysWatch?.length == daysOfTheWeek.length}
            onChange={() => {
              if (daysWatch?.length == daysOfTheWeek.length) {
                setValue('days', []);
              } else {
                setValue(
                  'days',
                  daysOfTheWeek.map(_ => _),
                );
              }
              if (errors.days) {
                trigger('days');
              }
            }}
          />
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

export default DaysOfWeekStep;
