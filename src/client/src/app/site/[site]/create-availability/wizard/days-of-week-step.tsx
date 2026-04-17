'use client';
import { BackLink, CheckBoxOrAll, FormGroup } from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import { daysOfTheWeek } from '@types';
import { Heading, Button, Checkboxes } from 'nhsuk-react-components';

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
        <span className="nhsuk-caption-l">Create weekly session</span>
        Select days to add to your weekly session
      </Heading>

      <p>You can create multiple weekly sessions, to cover:</p>
      <ul>
        <li>Vaccinator availability</li>
        <li>Type of vaccine available</li>
      </ul>
      <br />

      <FormGroup error={errors.days?.message}>
        <Checkboxes>
          {daysOfTheWeek.map(dayOfWeek => (
            <Checkboxes.Item
              id={`checkbox-${dayOfWeek.toLowerCase()}`}
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
            >
              {dayOfWeek}
            </Checkboxes.Item>
          ))}
          <CheckBoxOrAll />
          <Checkboxes.Item
            value={daysOfTheWeek}
            id={'allDays'}
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
          >
            Select all days
          </Checkboxes.Item>
        </Checkboxes>
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
