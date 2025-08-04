import {
  occurInOrder,
  parseToUkDatetime,
  RFC3339Format,
  ukNow,
} from '@services/timeService';
import * as yup from 'yup';

export interface DownloadReportFormValues {
  startDate: string;
  endDate: string;
}

export const REPORT_DATE_EARLIEST_ALLOWED = '2025-03-01';

export const downloadReportFormSchema: yup.ObjectSchema<DownloadReportFormValues> =
  yup
    .object({
      startDate: yup
        .string()
        .required('Enter a start date')
        .test(
          'is-after-february-2025-and-not-more-than-3-months-in-the-future',
          'Select a date on or after 1 March 2025 and within 3 months from today',
          value =>
            occurInOrder([
              parseToUkDatetime(REPORT_DATE_EARLIEST_ALLOWED, RFC3339Format),
              parseToUkDatetime(value, RFC3339Format),
              ukNow().add(3, 'month'),
            ]),
        ),
      endDate: yup
        .string()
        .required('Enter an end date')
        .test(
          'is-after-february-2025-and-not-more-than-3-months-in-the-future',
          'Select a date on or after 1 March 2025 and within 3 months from today',
          value =>
            occurInOrder([
              parseToUkDatetime(REPORT_DATE_EARLIEST_ALLOWED, RFC3339Format),
              parseToUkDatetime(value, RFC3339Format),
              ukNow().add(3, 'month'),
            ]),
        ),
    })
    .test(
      'end-date-after-start-date',
      'End date must be equal to or after start date',
      values => {
        if (!values.startDate || !values.endDate) {
          return true; // Validation will fail on required fields first
        }
        return occurInOrder([
          parseToUkDatetime(values.startDate, RFC3339Format),
          parseToUkDatetime(values.endDate, RFC3339Format),
        ]);
      },
    )
    .required();
