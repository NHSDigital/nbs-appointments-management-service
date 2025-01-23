import { DateComponents, TimeComponents } from '@types';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import utc from 'dayjs/plugin/utc';
import isoWeek from 'dayjs/plugin/isoWeek';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';

dayjs.extend(customParseFormat);
// Our times are treated as zone agnostic, but if we don't
// specify this then midnight 2020-09-16 will get formatted as 23:00 2020-09-15
dayjs.extend(utc);
dayjs.extend(isoWeek);
dayjs.extend(isSameOrBefore);

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

export const formatDateTimeToTime = (dateTime: string) => {
  const date = new Date(dateTime);

  const timeComponents: TimeComponents = {
    hour: date.getHours().toString(),
    minute: date.getMinutes().toString(),
  };

  return formatTimeString(timeComponents);
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

export const startOfWeek = (dateString: string) => {
  return dayjs(dateString).startOf('isoWeek').hour(0).minute(0).second(0);
};

export const endOfWeek = (dateString: string) => {
  return dayjs(dateString).endOf('isoWeek').hour(23).minute(59).second(59);
};

export const getWeek = (dateInWeek: dayjs.Dayjs): dayjs.Dayjs[] => {
  const week = [];

  const weekStart = dateInWeek.startOf('isoWeek');
  for (let i = 0; i < 7; i++) {
    week.push(weekStart.add(i, 'day'));
  }

  return week;
};

export const toTimeComponents = (time: string): TimeComponents | undefined => {
  const [hour, minute] = time.split(':');
  const parsedHour = Number(hour);
  const parsedMinute = Number(minute);

  if (!Number.isInteger(parsedHour) || !Number.isInteger(parsedMinute)) {
    return undefined;
  }

  return {
    hour: parsedHour,
    minute: parsedMinute,
  };
};

export const dateToString = (date: Date, format = 'D MMMM YYYY') => {
  return dayjs.utc(date).format(format);
};

export const isInTheFuture = (date: string, format = 'YYYY-MM-DD') => {
  const inputDate = dayjs(date, format);
  const today = dayjs().startOf('day');
  return inputDate.isAfter(today);
};

export const isBeforeOrEqual = (
  first: dayjs.Dayjs,
  second: dayjs.Dayjs,
  units: 'minute' = 'minute',
) => first.isSameOrBefore(second, units);

export const compareTimes = (
  first: TimeComponents,
  second: TimeComponents,
): 'earlier' | 'equal' | 'later' => {
  const minutesInFirstTime = Number(first.hour) * 60 + Number(first.minute);
  const minutesInSecondTime = Number(second.hour) * 60 + Number(second.minute);

  if (minutesInFirstTime < minutesInSecondTime) {
    return 'earlier';
  }
  if (minutesInFirstTime > minutesInSecondTime) {
    return 'later';
  }
  return 'equal';
};
