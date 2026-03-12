'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  FormGroup,
} from '@components/nhsuk-frontend';
import { DateInput, Heading } from 'nhsuk-react-components';
import { Controller, useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import {
  ukNow,
  addToUkDatetime,
  parseDateComponentsToUkDatetime,
  RFC3339Format,
} from '@services/timeService';
import { handlePositiveBoundedNumberInput } from '../create-availability/wizard/availability-template-wizard';
import { proposeCancelDateRange } from '@services/appointmentsService';
import { useState } from 'react';

interface Props {
  site: string;
}

const SelectDatesStep = ({
  site,
  goToNextStep,
  goToPreviousStep,
}: InjectedWizardProps & Props) => {
  const {
    control,
    trigger,
    getValues,
    setValue,
    formState: { errors },
  } = useFormContext<ChangeAvailabilityFormValues>();
  const [error, setError] = useState<Error | null>(null);

  if (error) {
    throw error;
  }

  const maxYear = addToUkDatetime(ukNow(), 1, 'year').year();

  const onContinue = async (e: React.FormEvent) => {
    e.preventDefault();

    const isStepValid = await trigger(['startDate', 'endDate']);

    if (isStepValid) {
      try {
        const { startDate, endDate } = getValues();

        const startDayjs = parseDateComponentsToUkDatetime(startDate);
        const endDayjs = parseDateComponentsToUkDatetime(endDate);

        if (!startDayjs || !endDayjs) throw new Error('Invalid dates');

        const response = await proposeCancelDateRange({
          site: site,
          from: startDayjs.format(RFC3339Format),
          to: endDayjs.format(RFC3339Format),
        });

        if (!response.success) return;

        setValue('proposedCancellationSummary', {
          sessionCount: response.data.sessionCount,
          bookingCount: response.data.bookingCount,
        });

        goToNextStep();
      } catch (err) {
        setError(err as Error);
      }
    }
  };

  return (
    <>
      <BackLink
        onClick={() => goToPreviousStep()}
        renderingStrategy="client"
        text="Back"
      />

      <Heading headingLevel="h2">Select dates to cancel</Heading>

      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          {/* Start Date Section */}
          <FormGroup error={errors.startDate?.message}>
            <DateInput
              legend="Start date"
              hint="For example, 15 3 2026"
              id="start-date-input"
            >
              <Controller
                control={control}
                name="startDate.day"
                render={({ field }) => (
                  <DateInput.Day
                    id="start-date-day"
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
                  <DateInput.Month
                    id="start-date-month"
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
                  <DateInput.Year
                    id="start-date-year"
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

          <br />

          {/* End Date Section */}
          <FormGroup error={errors.endDate?.message}>
            <DateInput
              legend="End date"
              hint="For example, 15 3 2024"
              id="end-date-input"
            >
              <Controller
                control={control}
                name="endDate.day"
                render={({ field }) => (
                  <DateInput.Day
                    id="end-date-day"
                    onChange={e =>
                      field.onChange(handlePositiveBoundedNumberInput(e, 31))
                    }
                    value={field.value ?? ''}
                  />
                )}
              />
              <Controller
                control={control}
                name="endDate.month"
                render={({ field }) => (
                  <DateInput.Month
                    id="end-date-month"
                    onChange={e =>
                      field.onChange(handlePositiveBoundedNumberInput(e, 12))
                    }
                    value={field.value ?? ''}
                  />
                )}
              />
              <Controller
                control={control}
                name="endDate.year"
                render={({ field }) => (
                  <DateInput.Year
                    id="end-date-year"
                    onChange={e =>
                      field.onChange(
                        handlePositiveBoundedNumberInput(e, maxYear),
                      )
                    }
                    value={field.value ?? ''}
                  />
                )}
              />
            </DateInput>
          </FormGroup>

          <ButtonGroup>
            <Button type="button" onClick={onContinue}>
              Continue
            </Button>
          </ButtonGroup>
        </div>
      </div>
    </>
  );
};

export default SelectDatesStep;
