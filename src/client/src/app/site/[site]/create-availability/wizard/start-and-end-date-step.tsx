'use client';
import {
  BackLink,
  Button,
  DateInput,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { Controller, useFormContext } from 'react-hook-form';
import {
  CreateAvailabilityFormValues,
  handlePositiveBoundedNumberInput,
} from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import {
  ukNow,
  parseDateComponentsToUkDatetime,
  isFutureCalendarDateUk,
  isAfterCalendarDateUk,
  isWithinNextCalendarYearUk,
  addToUkDatetime,
} from '@services/timeService';
import NhsHeading from '@components/nhs-heading';

const StartAndEndDateStep = ({
  stepNumber,
  goToNextStep,
  goToLastStep,
  setCurrentStep,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const { formState, trigger, getValues, control } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors, isValid: allStepsAreValid, touchedFields } = formState;

  const sessionType = getValues('sessionType');

  const validateFields = async () => {
    if (sessionType === 'single') {
      return trigger(['startDate']);
    } else {
      return trigger(['startDate', 'endDate']);
    }
  };

  const shouldSkipToSummaryStep =
    touchedFields.session?.services && allStepsAreValid;

  const onContinue = async () => {
    const formIsValid = await validateFields();
    if (!formIsValid) {
      return;
    }

    if (shouldSkipToSummaryStep) {
      goToLastStep();
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
      {sessionType === 'single' ? (
        <NhsHeading
          title="Add a date for your session"
          caption="Create single date session"
        />
      ) : (
        <NhsHeading
          title="Add start and end dates"
          caption="Create weekly session"
        />
      )}

      <FormGroup error={errors.startDate?.message}>
        <Controller
          name="startDate"
          control={control}
          rules={{
            validate: value => {
              const sessionDateDescriptor =
                sessionType === 'single' ? 'date' : 'start date';

              if (
                (value.day === '' || value.day === undefined) &&
                (value.month === '' || value.month === undefined) &&
                (value.year === '' || value.year === undefined)
              ) {
                return `Enter a ${sessionDateDescriptor}`;
              }

              const ukStartDate = parseDateComponentsToUkDatetime(value);

              if (ukStartDate === undefined) {
                return `Session ${sessionDateDescriptor} must be a valid date`;
              }

              const isFutureCalendarDate = isFutureCalendarDateUk(ukStartDate);

              if (!isFutureCalendarDate) {
                return `Session ${sessionDateDescriptor} must be in the future`;
              }

              const isWithinNextYear = isWithinNextCalendarYearUk(ukStartDate);

              if (!isWithinNextYear) {
                return `Session ${sessionDateDescriptor} must be within the next year`;
              }
            },
          }}
          render={() => (
            <DateInput
              heading={sessionType === 'single' ? 'Session date' : 'Start date'}
              hint="For example, 15 3 2024"
              id="start-date-input"
            >
              <Controller
                control={control}
                name="startDate.day"
                render={({ field }) => (
                  <TextInput
                    label="Day"
                    type="number"
                    id="start-date-input-day"
                    inputType="date"
                    onChange={e =>
                      field.onChange(handlePositiveBoundedNumberInput(e, 31))
                    }
                    value={field.value ?? ''}
                  />
                )}
              />

              <Controller
                control={control}
                name="startDate.month"
                render={({ field }) => (
                  <TextInput
                    label="Month"
                    type="number"
                    id="start-date-input-month"
                    inputType="date"
                    onChange={e =>
                      field.onChange(handlePositiveBoundedNumberInput(e, 12))
                    }
                    value={field.value ?? ''}
                  />
                )}
              />

              <Controller
                control={control}
                name="startDate.year"
                render={({ field }) => (
                  <TextInput
                    label="Year"
                    type="number"
                    id="start-date-input-year"
                    inputType="date"
                    width={3}
                    onChange={e =>
                      field.onChange(
                        handlePositiveBoundedNumberInput(
                          e,
                          addToUkDatetime(ukNow(), 1, 'year').year(),
                        ),
                      )
                    }
                    value={field.value ?? ''}
                  />
                )}
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
              if (
                (value.day === '' || value.day === undefined) &&
                (value.month === '' || value.month === undefined) &&
                (value.year === '' || value.year === undefined)
              ) {
                return `Enter an end date`;
              }

              const startDate = parseDateComponentsToUkDatetime(form.startDate);
              const endDate = parseDateComponentsToUkDatetime(value);

              if (endDate === undefined || startDate === undefined) {
                return 'Session end date must be a valid date';
              }

              const isStartAfterEnd = isAfterCalendarDateUk(startDate, endDate);

              if (isStartAfterEnd) {
                return 'Session end date must be after the start date';
              }

              const isWithinNextYear = isWithinNextCalendarYearUk(endDate);

              if (!isWithinNextYear) {
                return 'Session end date must be within the next year';
              }
            },
          }}
          render={() => (
            <>
              <br />
              <FormGroup error={errors.endDate?.message}>
                <DateInput
                  heading="End date"
                  hint="For example, 15 3 2024"
                  id="end-date-input"
                >
                  <Controller
                    control={control}
                    name="endDate.day"
                    render={({ field }) => (
                      <TextInput
                        label="Day"
                        type="number"
                        id="end-date-input-day"
                        inputType="date"
                        onChange={e =>
                          field.onChange(
                            handlePositiveBoundedNumberInput(e, 31),
                          )
                        }
                        value={field.value ?? ''}
                      />
                    )}
                  />

                  <Controller
                    control={control}
                    name="endDate.month"
                    render={({ field }) => (
                      <TextInput
                        label="Month"
                        type="number"
                        id="end-date-input-month"
                        inputType="date"
                        onChange={e =>
                          field.onChange(
                            handlePositiveBoundedNumberInput(e, 12),
                          )
                        }
                        value={field.value ?? ''}
                      />
                    )}
                  />

                  <Controller
                    control={control}
                    name="endDate.year"
                    render={({ field }) => (
                      <TextInput
                        label="Year"
                        type="number"
                        id="end-date-input-year"
                        inputType="date"
                        width={3}
                        onChange={e =>
                          field.onChange(
                            handlePositiveBoundedNumberInput(
                              e,
                              addToUkDatetime(ukNow(), 1, 'year').year(),
                            ),
                          )
                        }
                        value={field.value ?? ''}
                      />
                    )}
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
