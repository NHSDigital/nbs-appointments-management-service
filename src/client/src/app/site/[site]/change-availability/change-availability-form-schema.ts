import {
  occurInOrder,
  parseDateComponentsToUkDatetime,
  ukNow,
} from '@services/timeService';
import * as yup from 'yup';
import { DateComponents } from '@types';

export interface ChangeAvailabilityFormValues {
  startDate: DateComponents;
  endDate: DateComponents;
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
    })
    .test('date-range-logic', '', function (values) {
      const { startDate, endDate } = values;

      const start = parseDateComponentsToUkDatetime(
        startDate as DateComponents,
      );
      const end = parseDateComponentsToUkDatetime(endDate as DateComponents);

      if (
        !start ||
        !end ||
        !start.isAfter(ukNow(), 'day') ||
        !end.isAfter(ukNow(), 'day')
      ) {
        return true;
      }

      if (!occurInOrder([start, end])) {
        return this.createError({
          path: 'endDate',
          message: 'End date must be on or after the start date',
        });
      }

      const diffInDays = end.diff(start, 'day') + 1;

      if (diffInDays > maxDays) {
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
