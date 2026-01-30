'use client';
import NhsHeading from '@components/nhs-heading';
import { BackLink, Button, ButtonGroup } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import Datepicker from '@components/nhsuk-frontend/custom/datepicker';
import { Controller, useFormContext } from 'react-hook-form';
import { ReportsFormValues } from './reports-template-wizard';
import { REPORT_DATE_EARLIEST_ALLOWED } from '../download-report-form-schema';
import { RFC3339Format, ukNow } from '@services/timeService';

const ReportDateRangeStep = ({
  goToPreviousStep,
  goToNextStep,
}: InjectedWizardProps) => {
  const {
    control,
    formState: { errors },
  } = useFormContext<ReportsFormValues>();
  const today = ukNow();
  const onContinue = (e: React.FormEvent) => {
    e.preventDefault();
    goToNextStep();
  };
  return (
    <>
      <BackLink
        onClick={goToPreviousStep}
        renderingStrategy="client"
        text="Back"
      />
      <br />
      <NhsHeading title="Select the dates and run a report" />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-one-third">
          {errors.root && (
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error: </span>
              {errors.root.message}
            </span>
          )}

          <Controller
            name="startDate"
            control={control}
            render={({ field }) => (
              <Datepicker
                id="startDate"
                label="Start date"
                hint="For example, 17/05/2024."
                min={REPORT_DATE_EARLIEST_ALLOWED}
                max={today.add(3, 'month').format(RFC3339Format)}
                error={errors.startDate?.message}
                {...field}
              />
            )}
          />

          <Controller
            name="endDate"
            control={control}
            render={({ field }) => (
              <Datepicker
                id="endDate"
                label="End date"
                hint="For example, 17/05/2024."
                min={REPORT_DATE_EARLIEST_ALLOWED}
                max={today.add(3, 'month').format(RFC3339Format)}
                error={errors.endDate?.message}
                {...field}
              />
            )}
          />

          <ButtonGroup>
            <Button type="button" onClick={onContinue}>
              Create report
            </Button>
          </ButtonGroup>
        </div>
      </div>
    </>
  );
};

export default ReportDateRangeStep;
