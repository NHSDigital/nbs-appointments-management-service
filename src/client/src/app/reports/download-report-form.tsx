'use client';
import { RFC3339Format, ukNow } from '@services/timeService';
import {
  downloadReportFormSchema,
  DownloadReportFormValues,
  REPORT_DATE_EARLIEST_ALLOWED,
} from './download-report-form-schema';
import { yupResolver } from '@hookform/resolvers/yup';
import { Controller, useForm } from 'react-hook-form';
import { BackLink, Button, ButtonGroup } from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import Datepicker from '@components/nhsuk-frontend/custom/datepicker';
import { BackLinkProps } from '@components/nhsuk-frontend/back-link';
import FormWrapper from '@components/form-wrapper';

type DownloadReportFormProps = {
  setReportRequest: (reportRequest: DownloadReportFormValues) => void;
  backLink: BackLinkProps;
};

const DownloadReportForm = ({
  setReportRequest,
  backLink,
}: DownloadReportFormProps) => {
  const today = ukNow();
  const {
    handleSubmit,
    control,
    formState: { errors },
    setError,
  } = useForm<DownloadReportFormValues>({
    defaultValues: {
      startDate: today.format(RFC3339Format),
      endDate: today.format(RFC3339Format),
    },
    resolver: yupResolver(downloadReportFormSchema),
  });

  return (
    <>
      <BackLink {...backLink} />
      <br />
      <NhsHeading title="Select the dates and run a report" />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-one-third">
          <FormWrapper<DownloadReportFormValues>
            submitHandler={payload => {
              setReportRequest(payload);
            }}
            handleSubmit={handleSubmit}
            setError={setError}
            errors={errors}
          >
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
          </FormWrapper>
        </div>
      </div>
    </>
  );
};

export default DownloadReportForm;
