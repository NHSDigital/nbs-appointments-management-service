import { DateComponents, TimeComponents } from '@types';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import utc from 'dayjs/plugin/utc';

dayjs.extend(customParseFormat);
// Our times are treated as zone agnostic, but if we don't
// specify this then midnight 2020-09-16 will get formatted as 23:00 2020-09-15
dayjs.extend(utc);

export const now = () => dayjs.utc();

export const isValidDate = (
  day: string | number,
  month: string | number,
  year: string | number,
) => {
  const parsedDay = Number(day);
  const parsedMonth = Number(month);
  const parsedYear = Number(year);

  if (
    Number.isNaN(parsedDay) ||
    Number.isNaN(parsedMonth) ||
    Number.isNaN(parsedYear)
  ) {
    return false;
  }

  const inputString = `${toTwoDigitFormat(parsedDay)}-${toTwoDigitFormat(parsedMonth)}-${parsedYear}`;
  const potentialDate = dayjs.utc(inputString, 'DD-MM-YYYY', true);

  return potentialDate.isValid();
};

export const parseDateComponents = ({ day, month, year }: DateComponents) => {
  if (!isValidDate(day, month, year)) {
    return undefined;
  }

  const inputString = `${toTwoDigitFormat(day)}-${toTwoDigitFormat(month)}-${year}`;

  return dayjs.utc(inputString, 'DD-MM-YYYY', true);
};

export const isSameDayOrBefore = (
  firstDate: dayjs.Dayjs,
  secondDate: dayjs.Dayjs,
) => {
  return (
    firstDate.isSame(secondDate, 'day') || firstDate.isBefore(secondDate, 'day')
  );
};

export const formatTimeString = ({ hour, minute }: TimeComponents) => {
  const parsedHour = Number(hour);
  const parsedMinute = Number(minute);

  if (!Number.isInteger(parsedHour) || !Number.isInteger(parsedMinute)) {
    return undefined;
  }

  if (
    parsedHour < 0 ||
    parsedHour > 23 ||
    parsedMinute < 0 ||
    parsedMinute > 59
  ) {
    return undefined;
  }

  return `${toTwoDigitFormat(hour)}:${toTwoDigitFormat(minute)}`;
};

export const toTwoDigitFormat = (
  input: number | string,
): string | undefined => {
  const inputAsNumber = Number(input);
  if (inputAsNumber < 0 || inputAsNumber > 99 || Number.isNaN(inputAsNumber)) {
    return undefined;
  }

  const stringInput = `${input}`;
  return stringInput.length === 1 ? `0${stringInput}` : stringInput;
};

export const parseDateString = (dateString: string, format = 'YYYY-MM-DD') => {
  return dayjs.utc(dateString, format, true);
};