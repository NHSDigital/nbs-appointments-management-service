import { DateComponents, IsoTimezone, TimeComponents } from '@types';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import isoWeek from 'dayjs/plugin/isoWeek';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';

dayjs.extend(customParseFormat);

//only want utc date for passing dayjs props between server and client components (via session params)
//everything else should use 'Europe/London' where possible
dayjs.extend(utc);
dayjs.extend(timezone);
dayjs.extend(isoWeek);
dayjs.extend(isSameOrBefore);

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

export const isSameDayOrBefore = (
  firstDate: dayjs.Dayjs,
  secondDate: dayjs.Dayjs,
) => {
  return (
    firstDate.isSame(secondDate, 'day') || firstDate.isBefore(secondDate, 'day')
  );
};

export const formatUkDatetimeToTime = (dateTime: string) => {
  const date = parseDateStringToUkDatetime(dateTime, 'YYYY-MM-DDTHH:mm:ss');

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
  format = 'YYYY-MM-DD',
) => {
  return dayjs.tz(dateString, format, ukTimezone);
};

export const isoTimezoneToDayjs = (isoTimezone: IsoTimezone) => {
  return dayjs.tz(isoTimezone.iso, isoTimezone.tz);
};

export const ukStartOfWeek = (dateString: string) => {
  return dayjs.tz(dateString, ukTimezone).startOf('isoWeek');
};

export const ukEndOfWeek = (dateString: string) => {
  return dayjs.tz(dateString, ukTimezone).endOf('isoWeek');
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
  return dayjs.tz(date, ukTimezone).format(format);
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

//create new date from an operation on a provided UK date.
//new datetime should have the correct timezone and offset.
export const addToUkDate = (
  ukDatetime: dayjs.Dayjs,
  value: number,
  manipulateType: dayjs.ManipulateType,
  format: string,
): dayjs.Dayjs => {
  const stringifyDatetime = ukDatetime
    .add(value, manipulateType)
    .format(format);

  return parseDateStringToUkDatetime(stringifyDatetime, format);
};

export const getUkWeeksOfTheMonth = (
  ukDateInMonth: dayjs.Dayjs,
): dayjs.Dayjs[][] => {
  const startOfFirstWeekInMonth = ukDateInMonth
    .startOf('month')
    .startOf('isoWeek');
  const endOfLastWeekInMonth = ukDateInMonth.endOf('month').endOf('isoWeek');

  const dates: dayjs.Dayjs[][] = [];
  let currentWeek: dayjs.Dayjs[] = [];
  let currentDate = startOfFirstWeekInMonth;

  while (currentDate.isSameOrBefore(endOfLastWeekInMonth)) {
    currentWeek.push(currentDate);
    if (currentWeek.length === 7) {
      dates.push(currentWeek);
      currentWeek = [];
    }

    currentDate = addToUkDate(currentDate, 1, 'day', 'YYYY-MM-DD');
  }

  if (currentWeek.length > 0) {
    dates.push(currentWeek);
  }

  return dates;
};
