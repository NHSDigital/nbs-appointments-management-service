'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  DateInput,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import { Controller, useFormContext } from 'react-hook-form';
import { InjectedWizardProps } from '@components/wizard';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import { ukNow, addToUkDatetime } from '@services/timeService';
import { handlePositiveBoundedNumberInput } from '../create-availability/wizard/availability-template-wizard';

const SelectDatesStep = ({
  goToNextStep,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const {
    control,
    trigger,
    formState: { errors },
  } = useFormContext<ChangeAvailabilityFormValues>();
  const maxYear = addToUkDatetime(ukNow(), 1, 'year').year();

  const onContinue = async (e: React.FormEvent) => {
    e.preventDefault();

    const isStepValid = await trigger(['startDate', 'endDate']);

    if (isStepValid) {
      goToNextStep();
    }
  };

  return (
    <>
      <BackLink
        onClick={() => goToPreviousStep()}
        renderingStrategy="client"
        text="Back"
      />

      <NhsHeading title="Select dates to cancel" />

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
                  <TextInput
                    {...field}
                    label="Day"
                    type="number"
                    id="start-date-day"
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
                    {...field}
                    label="Month"
                    type="number"
                    id="start-date-month"
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
                    {...field}
                    label="Year"
                    type="number"
                    id="start-date-year"
                    inputType="date"
                    width={3}
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

          <br />

          {/* End Date Section */}
          <FormGroup error={errors.endDate?.message}>
            <DateInput
              legend="End date"
              hint="For example, 15 3 2026"
              id="end-date-input"
            >
              <Controller
                control={control}
                name="endDate.day"
                render={({ field }) => (
                  <TextInput
                    {...field}
                    label="Day"
                    type="number"
                    id="end-date-day"
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
                name="endDate.month"
                render={({ field }) => (
                  <TextInput
                    {...field}
                    label="Month"
                    type="number"
                    id="end-date-month"
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
                name="endDate.year"
                render={({ field }) => (
                  <TextInput
                    {...field}
                    label="Year"
                    type="number"
                    id="end-date-year"
                    inputType="date"
                    width={3}
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
