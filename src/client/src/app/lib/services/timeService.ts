import { DateComponents, TimeComponents } from '@types';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';
import updateLocale from 'dayjs/plugin/updateLocale';

export const dayStringFormat = 'YYYY-MM-DD';
export const dateTimeStringFormat = 'YYYY-MM-DDTHH:mm:ss';

dayjs.extend(customParseFormat);

//only want utc date for passing dayjs props between server and client components (via session params)
//everything else should use 'Europe/London' where possible
dayjs.extend(utc);
dayjs.extend(timezone);
dayjs.extend(isSameOrBefore);
dayjs.extend(updateLocale);

// Set UK-style weeks: Monday as start
dayjs.updateLocale('en', {
  weekStart: 1,
});

// Set default locale if needed
dayjs.locale('en');

export const ukTimezone = 'Europe/London';

const utcNow = () => dayjs.utc();
export const ukNow = () => dayjs.tz(utcNow(), ukTimezone);

export const isValidUkDate = (
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
  const potentialDate = parseDateStringToUkDatetime(inputString, 'DD-MM-YYYY');

  return potentialDate.isValid();
};

export const parseDateComponentsToUkDatetime = ({
  day,
  month,
  year,
}: DateComponents) => {
  if (!isValidUkDate(day, month, year)) {
    return undefined;
  }

  const inputString = `${toTwoDigitFormat(day)}-${toTwoDigitFormat(month)}-${year}`;

  return parseDateStringToUkDatetime(inputString, 'DD-MM-YYYY');
};

export const formatUkDatetimeToTime = (dateTime: string) => {
  const date = parseDateStringToUkDatetime(dateTime, dateTimeStringFormat);

  const timeComponents: TimeComponents = {
    hour: date.hour(),
    minute: date.minute(),
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

export const parseDateStringToUkDatetime = (
  dateString: string,
  format = dayStringFormat,
) => {
  return dayjs.tz(dateString, format, ukTimezone);
};

export const getWeek = (dateInWeek: dayjs.Dayjs): dayjs.Dayjs[] => {
  const week = [];

  const weekStart = startOfUkWeek(dateInWeek);
  for (let i = 0; i < 7; i++) {
    week.push(addToUkDate(weekStart, i, 'day'));
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
  return dayjs.tz(date, ukTimezone).format(format);
};

//TODO rewrite??
export const isInTheFuture = (date: string, format = dayStringFormat) => {
  const inputDate = dayjs(date, format);
  const today = dayjs().startOf('day');
  return isAfter(inputDate, today);
};

export const isBeforeOrEqual = (
  first: dayjs.Dayjs,
  second: dayjs.Dayjs,
  unit?: dayjs.OpUnitType,
) => dayjs.utc(first).isSameOrBefore(dayjs.utc(second), unit);

export const isBefore = (
  first: dayjs.Dayjs,
  second: dayjs.Dayjs,
  unit?: dayjs.OpUnitType,
) => dayjs.utc(first).isBefore(dayjs.utc(second), unit);

export const isAfter = (
  first: dayjs.Dayjs,
  second: dayjs.Dayjs,
  unit?: dayjs.OpUnitType,
) => dayjs.utc(first).isAfter(dayjs.utc(second), unit);

export const isEqual = (first: dayjs.Dayjs, second: dayjs.Dayjs): boolean => {
  return dayjs.utc(first).isSame(dayjs.utc(second));
};

export const isSameUkDay = (
  first: dayjs.Dayjs,
  second: dayjs.Dayjs,
): boolean => {
  return dayjs
    .tz(first, ukTimezone)
    .isSame(dayjs.tz(second, ukTimezone), 'day');
};

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

//create new date from an operation on a provided UK date.
//new datetime should have the correct timezone and offset.
export const addToUkDate = (
  ukDatetime: dayjs.Dayjs,
  value: number,
  // manipulateType: dayjs.ManipulateType,
  manipulateType: 'day' | 'week' | 'hour' | 'minute',
  format = dayStringFormat,
): dayjs.Dayjs => {
  //have to do operation in UTC and stringify and parse to ensure the new created datetime has the UK timezone info correct
  //as simply doing .add() in daysjs maintains the original timezone info (even if the operation crosses a DST)
  // const utcDatetime = ukDatetime.utc();
  // const addedUtcDatetime = utcDatetime.add(value, manipulateType);
  // const addedUkDatetime = addedUtcDatetime.tz(ukTimezone);

  // const corrected = dayjs.tz(
  //   addedUkDatetime.format('YYYY-MM-DDTHH:mm:ss'),
  //   ukTimezone,
  // );

  // const stringifyDatetime = corrected.format(format);

  // const shifted = ukDatetime.add(value, manipulateType);
  // //forced reevaluation of timezone using solely Date and Time info
  // const corrected = dayjs.tz(shifted.format('YYYY-MM-DDTHH:mm:ss'), ukTimezone);
  // const stringifyDatetime = corrected.format(format);

  let shifted: dayjs.Dayjs = dayjs();

  switch (manipulateType) {
    case 'day':
      shifted = ukDatetime.date(ukDatetime.date() + value);
      break;
    case 'week':
      shifted = ukDatetime.date(ukDatetime.date() + value * 7);
      break;
    case 'hour':
      shifted = ukDatetime.hour(ukDatetime.hour() + value);
      break;
    case 'minute':
      shifted = ukDatetime.minute(ukDatetime.minute() + value);
      break;
    default:
      throw new Error('Unsupported');
  }

  //forced reevaluation of timezone using Date and Time info, to account for if the shift crossed a DST boundary
  const corrected = dayjs.tz(shifted.format('YYYY-MM-DDTHH:mm:ss'), ukTimezone);
  const stringifyDatetime = corrected.format(format);

  return parseDateStringToUkDatetime(stringifyDatetime, format);
};

export const startOfUkWeek = (ukDate: string | dayjs.Dayjs): dayjs.Dayjs => {
  if (typeof ukDate === 'string') {
    const ukDateTime = parseDateStringToUkDatetime(ukDate);
    return startOfUkWeek(ukDateTime);
  }

  return ukDate.startOf('week');
};

export const endOfUkWeek = (ukDate: string | dayjs.Dayjs): dayjs.Dayjs => {
  if (typeof ukDate === 'string') {
    const ukDateTime = parseDateStringToUkDatetime(ukDate);
    return endOfUkWeek(ukDateTime);
  }

  return ukDate.endOf('week');
};

//build a dayjs with the correct timezone info for the provided day and session start hours and minutes
export const buildUkSessionDatetime = (
  ukDay: dayjs.Dayjs,
  hours: number,
  minutes: number,
): dayjs.Dayjs => {
  //have to stringify and parse to ensure the new created datetime has the UK timezone info correct
  //as simply doing .add() in daysjs maintains the original timezone info (even if the operation crosses a DST)

  const newHour = addToUkDate(ukDay, hours, 'hour', dateTimeStringFormat);
  return addToUkDate(newHour, minutes, 'minute', dateTimeStringFormat);

  // const stringifyDatetime = ukStartOfDay
  //   .add(hours, 'hour')
  //   .add(minutes, 'minute')
  //   .format(dateTimeStringFormat);

  // return parseDateStringToUkDatetime(stringifyDatetime, dateTimeStringFormat);
};

//extract the string version of the ukDatetime out into a dayjs object with the correct timezone
export const extractUkSessionDatetime = (datetime: string) => {
  return parseDateStringToUkDatetime(datetime, dateTimeStringFormat);
};

export const getUkWeeksOfTheMonth = (
  ukDateInMonth: dayjs.Dayjs,
): dayjs.Dayjs[][] => {
  const startOfFirstWeekInMonth = startOfUkWeek(ukDateInMonth.startOf('month'));
  const endOfLastWeekInMonth = endOfUkWeek(ukDateInMonth.endOf('month'));

  const dates: dayjs.Dayjs[][] = [];
  let currentWeek: dayjs.Dayjs[] = [];
  let currentDate = startOfFirstWeekInMonth;

  while (isBeforeOrEqual(currentDate, endOfLastWeekInMonth)) {
    currentWeek.push(currentDate);
    if (currentWeek.length === 7) {
      dates.push(currentWeek);
      currentWeek = [];
    }

    currentDate = addToUkDate(currentDate, 1, 'day');
  }

  if (currentWeek.length > 0) {
    dates.push(currentWeek);
  }

  return dates;
};
