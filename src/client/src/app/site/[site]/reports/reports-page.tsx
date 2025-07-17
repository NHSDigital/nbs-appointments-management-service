/* eslint-disable react/jsx-props-no-spreading */
'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  ButtonGroup,
  InsetText,
} from '@nhsuk-frontend-components';
import { parseToUkDatetime, RFC3339Format, ukNow } from '@services/timeService';
import { Site } from '@types';
import { useState } from 'react';
import { Controller, SubmitHandler, useForm } from 'react-hook-form';
import {
  DownloadReportFormValues,
  REPORT_DATE_EARLIEST_ALLOWED,
} from './download-report-form-schema';
import Datepicker from '@components/nhsuk-frontend/custom/datepicker';

interface ReportsPageProps {
  site: Site;
}

export const ReportsPage = ({ site }: ReportsPageProps) => {
  const today = ukNow();
  //const [step, setStep] = useState<'form' | 'download-button'>('form');
  const [downloadText, setDownloadText] = useState<string | undefined>(
    undefined,
  );
  const {
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<DownloadReportFormValues>({
    defaultValues: {
      startDate: today.format(RFC3339Format),
      endDate: today.format(RFC3339Format),
    },
  });

  const submitForm: SubmitHandler<DownloadReportFormValues> = async (
    form: DownloadReportFormValues,
  ) => {
    setDownloadText(
      `Download all data between ${parseToUkDatetime(form.startDate).format('dddd, D MMMM YYYY')} and ${parseToUkDatetime(form.endDate).format('dddd, D MMMM YYYY')}`,
    );
  };

  return (
    <>
      <BackLink
        renderingStrategy={'server'}
        href={`/site/${site.id}`}
        text={'Back'}
      />
      <NhsHeading title="Select the dates and create a report" />
      <form onSubmit={handleSubmit(submitForm)}>
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
          <Button type="submit">Create report</Button>
        </ButtonGroup>
      </form>
      {downloadText && <InsetText>{downloadText}</InsetText>}
    </>
  );
};
