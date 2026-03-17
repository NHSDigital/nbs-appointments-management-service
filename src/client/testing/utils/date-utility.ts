import {
  addToUkDatetime,
  RFC3339Format,
  endOfUkWeek,
  parseToUkDatetime,
  startOfUkWeek,
  ukNow,
  parseDateComponentsToUkDatetime,
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
  requiredformat = RFC3339Format,
) =>
  addToUkDatetime(ukNow(), numberOfDaysFromToday, 'day').format(requiredformat);

export const weekHeaderText = (date: string) =>
  `${startOfUkWeek(parseToUkDatetime(date)).format('D MMMM')} to ${endOfUkWeek(parseToUkDatetime(date)).format('D MMMM')}`;

export const getLongDayDateText = (dateComponents: DateComponents): string => {
  const dayjsDate = parseDateComponentsToUkDatetime(dateComponents);

  if (!dayjsDate || !dayjsDate.isValid()) {
    return '';
  }

  return dayjsDate.format('dddd D MMMM');
};
