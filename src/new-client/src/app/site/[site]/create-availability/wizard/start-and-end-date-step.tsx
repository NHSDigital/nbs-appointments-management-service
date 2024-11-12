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
  isSameDayOrBefore,
  now,
  parseDateComponents,
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

              if (startDate.isBefore(now().add(1, 'day'), 'day')) {
                return 'Session date must be in the future';
              }

              if (startDate.isAfter(now().add(1, 'year'), 'day')) {
                return 'Session date must be within the next year';
              }
            },
          }}
          render={() => (
            <DateInput
              heading="Start date"
              hint="For example, 15 3 1984"
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
                          now().add(1, 'year').year(),
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
              const endDate = parseDateComponents(value);
              const startDate = parseDateComponents(form.startDate);

              if (endDate === undefined || startDate === undefined) {
                return 'Session end date must be a valid date';
              }

              if (!isSameDayOrBefore(startDate, endDate)) {
                return 'Session end date must be after the start date';
              }

              if (endDate.isAfter(now().add(1, 'year'), 'day')) {
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
                  hint="For example, 15 3 1984"
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
                              now().add(1, 'year').year(),
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
