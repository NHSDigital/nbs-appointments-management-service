'use client';
import {
  BackLink,
  Button,
  DateInput,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { Controller, useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import {
  isSameDayOrBefore,
  now,
  parseDateComponents,
} from '@services/timeService';
import NhsHeading from '@components/nhs-heading';

const StartAndEndDateStep = ({
  stepNumber,
  goToNextStep,
  setCurrentStep,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const { register, formState, trigger, getValues, control } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const sessionType = getValues('sessionType');

  const validateFields = async () => {
    if (sessionType === 'single') {
      return trigger(['startDate']);
    } else {
      return trigger(['startDate', 'endDate']);
    }
  };

  const onContinue = async () => {
    const formIsValid = await validateFields();
    if (!formIsValid) {
      return;
    }

    if (getValues('sessionType') === 'repeating') {
      goToNextStep();
    } else {
      setCurrentStep(stepNumber + 2);
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
        title={
          sessionType === 'single'
            ? 'Session date'
            : 'Add start and end dates for your availability period'
        }
        caption="Create availability period"
      />
      <FormGroup error={errors.startDate?.message}>
        <Controller
          name="startDate"
          control={control}
          rules={{
            validate: value => {
              const startDate = parseDateComponents(value);

              if (startDate === undefined) {
                return 'Session date must be a valid date';
              }

              if (startDate.isBefore(now(), 'day')) {
                return 'Session date must be in the future';
              }
            },
          }}
          render={() => (
            <DateInput
              heading="Start date"
              hint="For example, 15 3 1984"
              id="start-date-input"
            >
              <TextInput
                label="Day"
                type="number"
                id="start-date-input-day"
                inputType="date"
                {...register('startDate.day', {
                  valueAsNumber: true,
                })}
              />
              <TextInput
                label="Month"
                type="number"
                id="start-date-input-month"
                inputType="date"
                {...register('startDate.month', {
                  valueAsNumber: true,
                })}
              />
              <TextInput
                label="Year"
                type="number"
                id="start-date-input-year"
                inputType="date"
                width={3}
                {...register('startDate.year', {
                  valueAsNumber: true,
                })}
              />
            </DateInput>
          )}
        />
      </FormGroup>
      {getValues('sessionType') === 'repeating' && (
        <Controller
          name="endDate"
          control={control}
          rules={{
            validate: (value, form) => {
              const endDate = parseDateComponents(value);
              const startDate = parseDateComponents(form.startDate);

              if (endDate === undefined || startDate === undefined) {
                return 'Session end date must be a valid date';
              }

              if (!isSameDayOrBefore(startDate, endDate)) {
                return 'Session end date must be after the start date';
              }
            },
          }}
          render={() => (
            <>
              <br />
              <FormGroup error={errors.endDate?.message}>
                <DateInput
                  heading="End date"
                  hint="For example, 15 3 1984"
                  id="end-date-input"
                >
                  <TextInput
                    label="Day"
                    type="number"
                    id="end-date-input-day"
                    inputType="date"
                    {...register('endDate.day', {
                      valueAsNumber: true,
                    })}
                  />
                  <TextInput
                    label="Month"
                    type="number"
                    id="end-date-input-month"
                    inputType="date"
                    width={3}
                    {...register('endDate.month', {
                      valueAsNumber: true,
                    })}
                  />
                  <TextInput
                    label="Year"
                    type="number"
                    id="end-date-input-year"
                    inputType="date"
                    {...register('endDate.year', {
                      valueAsNumber: true,
                    })}
                  />
                </DateInput>
              </FormGroup>
            </>
          )}
        ></Controller>
      )}

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

export default StartAndEndDateStep;
