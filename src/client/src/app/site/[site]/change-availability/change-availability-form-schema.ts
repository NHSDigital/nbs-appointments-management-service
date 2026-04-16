import {
  occurInOrder,
  parseDateComponentsToUkDatetime,
  ukNow,
  addToUkDatetime,
  isAfterCalendarDateUk,
  isBeforeCalendarDateUk,
} from '@services/timeService';
import * as yup from 'yup';
import { DateComponents } from '@types';
import {
  CancelDateRangeResponse,
  ProposeCancelDateRangeResponse,
} from '@types';

export interface ChangeAvailabilityFormValues {
  startDate: DateComponents;
  endDate: DateComponents;
  proposedCancellationSummary?: ProposeCancelDateRangeResponse;
  cancellationSummary?: CancelDateRangeResponse;
  cancellationDecision?: 'keep-bookings' | 'cancel-bookings';
}

const isDateEmpty = (date?: Partial<DateComponents>) =>
  !date ||
  ((date.day === '' || date.day === undefined) &&
    (date.month === '' || date.month === undefined) &&
    (date.year === '' || date.year === undefined));

const dateComponentsSchema = (
  label: string,
  requiredMsg: string,
  futureMsg: string,
) =>
  yup
    .object({
      day: yup.string().default(''),
      month: yup.string().default(''),
      year: yup.string().default(''),
    })
    .test('is-complete', requiredMsg, value => !isDateEmpty(value))
    .test('is-valid', `Enter a valid ${label}`, value => {
      if (isDateEmpty(value)) return true;
      return !!parseDateComponentsToUkDatetime(value as DateComponents);
    })
    .test('is-future', futureMsg, value => {
      const date = parseDateComponentsToUkDatetime(value as DateComponents);
      if (!date) return true;
      return date.isAfter(ukNow(), 'day');
    });

export const createChangeAvailabilityFormSchema = (maxDays: number) => {
  return yup
    .object({
      startDate: dateComponentsSchema(
        'start date',
        'Enter a start date',
        'Start date must be in the future',
      ),
      endDate: dateComponentsSchema(
        'end date',
        'Enter an end date',
        'End date must be in the future',
      ),
      cancellationDecision: yup
        .string()
        .nullable()
        .oneOf(
          ['keep-bookings', 'cancel-bookings'],
          'Please select a valid cancellation option',
        )
        .required('Select what you want to do with the bookings'),
    })
    .test('date-range-logic', '', function (values) {
      const { startDate, endDate } = values;

      const start = parseDateComponentsToUkDatetime(
        startDate as DateComponents,
      );
      const end = parseDateComponentsToUkDatetime(endDate as DateComponents);

      const ukDateTime = ukNow();

      if (
        !start ||
        !end ||
        !isAfterCalendarDateUk(start, ukDateTime) ||
        !isAfterCalendarDateUk(end, ukDateTime)
      ) {
        return true;
      }

      //TODO this seems expensive to check greater than logic??
      if (!occurInOrder([start, end])) {
        return this.createError({
          path: 'endDate',
          message: 'End date must be on or after the start date',
        });
      }

      const allowedEnd = addToUkDatetime(start, maxDays, 'day');

      if (!isBeforeCalendarDateUk(end, allowedEnd)) {
        return new yup.ValidationError([
          this.createError({
            path: 'startDate',
            message: `Start date must be within ${maxDays} days of the end date`,
          }),
          this.createError({
            path: 'endDate',
            message: `End date must be within ${maxDays} days of the start date`,
          }),
        ]);
      }

      return true;
    })
    .required() as yup.ObjectSchema<ChangeAvailabilityFormValues>;
};
