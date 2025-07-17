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
        <Controller
          name="startDate"
          control={control}
          render={({ field }) => (
            <div className="nhsuk-form-group">
              <label className="nhsuk-label" htmlFor="startDate">
                Start date
              </label>
              <div id="startDate-hint" className="nhsuk-hint">
                For example, 17/05/2024.
              </div>
              {errors.startDate && (
                <span className="nhsuk-error-message">
                  <span className="nhsuk-u-visually-hidden">Error: </span>
                  {errors.startDate.message}
                </span>
              )}

              <input
                className="nhsuk-input nhsuk-date-input__input nhsuk-input--width-8"
                id="startDate"
                type="date"
                min={REPORT_DATE_EARLIEST_ALLOWED}
                max={today.add(3, 'month').format(RFC3339Format)}
                inputMode="numeric"
                aria-describedby="startDate-hint"
                autoComplete="off"
                {...field}
              />
            </div>
          )}
        />
        <Controller
          name="endDate"
          control={control}
          render={({ field }) => (
            <div className="nhsuk-form-group">
              <label className="nhsuk-label" htmlFor="endDate">
                End date
              </label>
              <div id="endDate-hint" className="nhsuk-hint">
                For example, 17/05/2024.
              </div>
              {errors.endDate && (
                <span className="nhsuk-error-message">
                  <span className="nhsuk-u-visually-hidden">Error: </span>
                  {errors.endDate.message}
                </span>
              )}

              <input
                className="nhsuk-input nhsuk-date-input__input nhsuk-input--width-8"
                id="endDate"
                type="date"
                min={REPORT_DATE_EARLIEST_ALLOWED}
                max={today.add(3, 'month').format(RFC3339Format)}
                inputMode="numeric"
                aria-describedby="endDate-hint"
                autoComplete="off"
                {...field}
              />
            </div>
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
