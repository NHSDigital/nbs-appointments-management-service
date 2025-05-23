import {
  addToUkDatetime,
  dateFormat,
  endOfUkWeek,
  parseToUkDatetime,
  startOfUkWeek,
  ukNow,
} from '@services/timeService';
import { DateComponents } from '@types';

export const getDateInFuture = (
  numberOfDaysFromToday: number,
): DateComponents => {
  const futureDate = addToUkDatetime(ukNow(), numberOfDaysFromToday, 'day');

  return {
    day: futureDate.format('DD'),
    month: futureDate.format('MM'),
    year: futureDate.format('YYYY'),
  };
};

export const daysFromToday = (
  numberOfDaysFromToday = 1,
  requiredformat = dateFormat,
) =>
  addToUkDatetime(ukNow(), numberOfDaysFromToday, 'day').format(requiredformat);

export const weekHeaderText = (date: string) =>
  `${startOfUkWeek(parseToUkDatetime(date)).format('D MMMM')} to ${endOfUkWeek(parseToUkDatetime(date)).format('D MMMM')}`;
