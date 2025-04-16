import {
  dayStringFormat,
  endOfUkWeek,
  startOfUkWeek,
} from '@services/timeService';
import dayjs from 'dayjs';

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
  `${startOfUkWeek(dayjs(date)).format('D MMMM')} to ${endOfUkWeek(dayjs(date)).format('D MMMM')}`;
