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
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const { register, setValue, watch, formState, trigger } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const daysWatch = watch('days');

  const onContinue = async () => {
    const formIsValid = await trigger(['days']);
    if (!formIsValid) {
      return;
    }

    goToNextStep();
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
        title="Select days that you want to add to your availability period"
        caption="Create availability period"
      />

      <FormGroup error={errors.days?.message}>
        <div className="nhsuk-hint" id="example-hint">
          You can add multiple repeat sessions to this availability period, to
          cover part time work or different service types.
        </div>
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
                trigger('days');
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
              trigger('days');
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
