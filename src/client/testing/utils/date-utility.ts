import { dayStringFormat } from '@services/timeService';
import dayjs from 'dayjs';
import isoWeek from 'dayjs/plugin/isoWeek';
dayjs.extend(isoWeek);

interface DateComponents {
  day: string;
  month: string;
  year: string;
}

export const getDateInFuture = (
  numberOfDaysFromToday: number,
): DateComponents => {
  const futureDate = dayjs().add(numberOfDaysFromToday, 'day');

  return {
    day: futureDate.format('DD'),
    month: futureDate.format('MM'),
    year: futureDate.format('YYYY'),
  };
};

export const daysFromToday = (
  numberOfDaysFromToday = 1,
  requiredformat = dayStringFormat,
) => dayjs().add(numberOfDaysFromToday, 'day').format(requiredformat);

export const weekHeaderText = (date: string) =>
  `${dayjs(date).startOf('isoWeek').format('D MMMM')} to ${dayjs(date).endOf('isoWeek').format('D MMMM')}`;
