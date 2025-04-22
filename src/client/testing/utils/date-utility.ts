import {
  addToUkDate,
  dayStringFormat,
  endOfUkWeek,
  parseDateStringToUkDatetime,
  startOfUkWeek,
  ukNow,
} from '@services/timeService';
import { DateComponents } from '@types';

export const getDateInFuture = (
  numberOfDaysFromToday: number,
): DateComponents => {
  const futureDate = addToUkDate(ukNow(), numberOfDaysFromToday, 'day');

  return {
    day: futureDate.format('DD'),
    month: futureDate.format('MM'),
    year: futureDate.format('YYYY'),
  };
};

export const daysFromToday = (
  numberOfDaysFromToday = 1,
  requiredformat = dayStringFormat,
) => addToUkDate(ukNow(), numberOfDaysFromToday, 'day').format(requiredformat);

export const weekHeaderText = (date: string) =>
  `${startOfUkWeek(parseDateStringToUkDatetime(date)).format('D MMMM')} to ${endOfUkWeek(parseDateStringToUkDatetime(date)).format('D MMMM')}`;
