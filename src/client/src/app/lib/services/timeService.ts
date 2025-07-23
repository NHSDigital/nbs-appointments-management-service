import { DateComponents, TimeComponents } from '@types';
import dayjs from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';
import updateLocale from 'dayjs/plugin/updateLocale';

export const RFC3339Format = 'YYYY-MM-DD';
export const dateTimeFormat = 'YYYY-MM-DDTHH:mm:ss';

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

export type DayJsType = dayjs.Dayjs;

//#region Formatter Methods

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

export const toTimeFormat = (
  input: string | TimeComponents,
): string | undefined => {
  if (typeof input === 'string') {
    const date = parseToUkDatetime(input, dateTimeFormat);

    const timeComponents: TimeComponents = {
      hour: date.hour(),
      minute: date.minute(),
    };

    return toTimeFormat(timeComponents);
  }

  const parsedHour = Number(input.hour);
  const parsedMinute = Number(input.minute);

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

  return `${toTwoDigitFormat(input.hour)}:${toTwoDigitFormat(input.minute)}`;
};

export const jsDateFormat = (date: Date, format = 'D MMMM YYYY') => {
  return dayjs.tz(date, ukTimezone).format(format);
};

//#endregion

//#region Parser Methods

export const parseToTimeComponents = (
  time: string,
): TimeComponents | undefined => {
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

export const parseToUkDatetime = (
  input: string | Date,
  format = RFC3339Format,
): DayJsType => {
  return dayjs.tz(input, format, ukTimezone);
};

export const parseDateComponentsToUkDatetime = ({
  day,
  month,
  year,
}: DateComponents) => {
  if (!isValidDate(day, month, year)) {
    return undefined;
  }
  const inputString = `${toTwoDigitFormat(day)}-${toTwoDigitFormat(month)}-${year}`;
  return parseToUkDatetime(inputString, 'DD-MM-YYYY');
};

//#endregion

//#region Operator Methods

// All datetime strict equality checks need converting to UTC time first, then comparing.
// dayJs seems to have an issue with equality checks (especially if the node server is in a different timezone) for datetime objects across timezones, even if they ARE the same
// see: https://github.com/iamkun/dayjs/issues/1189
export const isBeforeOrEqual = (first: dayjs.Dayjs, second: dayjs.Dayjs) =>
  dayjs.utc(first).isSameOrBefore(dayjs.utc(second));

export const isBefore = (first: dayjs.Dayjs, second: dayjs.Dayjs) =>
  dayjs.utc(first).isBefore(dayjs.utc(second));

export const isAfter = (first: dayjs.Dayjs, second: dayjs.Dayjs) =>
  dayjs.utc(first).isAfter(dayjs.utc(second));

export const isEqual = (first: dayjs.Dayjs, second: dayjs.Dayjs): boolean => {
  return dayjs.utc(first).isSame(dayjs.utc(second));
};

export const isAfterCalendarDateUk = (
  firstUkDatetime: dayjs.Dayjs,
  secondUkDatetime: dayjs.Dayjs,
) => {
  return isAfter(
    firstUkDatetime.startOf('day'),
    secondUkDatetime.startOf('day'),
  );
};

export const occurInOrder = (ukDates: dayjs.Dayjs[]) => {
  for (let index = 1; index < ukDates.length; index++) {
    if (
      !(
        isEqual(ukDates[index], ukDates[index - 1]) ||
        isAfter(ukDates[index], ukDates[index - 1])
      )
    ) {
      return false;
    }
  }
  return true;
};

export const isBeforeOrEqualCalendarDateUk = (
  firstUkDatetime: dayjs.Dayjs,
  secondUkDatetime: dayjs.Dayjs,
) => {
  return isBeforeOrEqual(
    firstUkDatetime.startOf('day'),
    secondUkDatetime.startOf('day'),
  );
};

export const isFutureCalendarDateUk = (ukDatetime: dayjs.Dayjs) => {
  return isAfterCalendarDateUk(ukDatetime, ukNow());
};

export const isWithinNextCalendarYearUk = (ukDatetime: dayjs.Dayjs) => {
  return isBeforeOrEqualCalendarDateUk(
    ukDatetime,
    addToUkDatetime(ukNow(), 1, 'year'),
  );
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

//#endregion

//when checking whether two datetimes occur on the same UK day
export const isOnTheSameUkDay = (
  first: dayjs.Dayjs,
  second: dayjs.Dayjs,
): boolean => {
  //dayJs 'isSame' DOES NOT WORK HERE!! this is due to a timezone asymmetry issue (even when both converted to same tz)
  //i.e a.isSame(b, 'day') !== b.isSame(a, 'day') in some cases!

  //they must first each be converted to Uk timezone, then day format compared.
  const firstUk = dayjs.tz(first, ukTimezone);
  const secondUk = dayjs.tz(second, ukTimezone);
  return firstUk.format(RFC3339Format) === secondUk.format(RFC3339Format);
};

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

  //have to verify using strict UTC rules
  const potentialDate = dayjs.utc(inputString, 'DD-MM-YYYY', true);

  return potentialDate.isValid();
};

export const getUkWeeksOfTheMonth = (ukDate: dayjs.Dayjs): dayjs.Dayjs[][] => {
  const startOfFirstWeekInMonth = startOfUkWeek(ukDate.startOf('month'));
  const endOfLastWeekInMonth = endOfUkWeek(ukDate.endOf('month'));

  const dates: dayjs.Dayjs[][] = [];
  let currentWeek: dayjs.Dayjs[] = [];
  let currentDate = startOfFirstWeekInMonth;

  while (isBeforeOrEqual(currentDate, endOfLastWeekInMonth)) {
    currentWeek.push(currentDate);
    if (currentWeek.length === 7) {
      dates.push(currentWeek);
      currentWeek = [];
    }

    currentDate = addToUkDatetime(currentDate, 1, 'day');
  }

  if (currentWeek.length > 0) {
    dates.push(currentWeek);
  }

  return dates;
};

export const getWeek = (dateInWeek: dayjs.Dayjs): dayjs.Dayjs[] => {
  const week = [];

  const weekStart = startOfUkWeek(dateInWeek);
  for (let i = 0; i < 7; i++) {
    week.push(addToUkDatetime(weekStart, i, 'day'));
  }

  return week;
};

//create new date from an operation on a provided UK date.
//new datetime should have the correct timezone and offset.
export const addToUkDatetime = (
  ukDatetime: dayjs.Dayjs,
  value: number,
  manipulateType: 'year' | 'day' | 'week' | 'hour' | 'minute',
  format = RFC3339Format,
): dayjs.Dayjs => {
  //we CAN'T just use dayJs.add method for this alone, as it DOES NOT work when the operation crosses a DST boundary.
  //(it preserves the original timezone rather than adjusting/re-evaluating)

  //the below code DOES NOT pass jest tests when ran in TZ="Pacific/Kiritimati"
  //const shifted = ukDatetime.add(value, manipulateType);

  let shifted: dayjs.Dayjs = dayjs();

  switch (manipulateType) {
    case 'year':
      shifted = ukDatetime.year(ukDatetime.year() + value);
      break;
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
  const correctedDatetime = dayjs.tz(
    shifted.format(dateTimeFormat),
    ukTimezone,
  );

  const stringifyDatetime = correctedDatetime.format(format);
  return parseToUkDatetime(stringifyDatetime, format);
};

export const startOfUkWeek = (ukDate: string | dayjs.Dayjs): dayjs.Dayjs => {
  if (typeof ukDate === 'string') {
    const ukDateTime = parseToUkDatetime(ukDate);
    return startOfUkWeek(ukDateTime);
  }

  return ukDate.startOf('week');
};

export const endOfUkWeek = (ukDate: string | dayjs.Dayjs): dayjs.Dayjs => {
  if (typeof ukDate === 'string') {
    const ukDateTime = parseToUkDatetime(ukDate);
    return endOfUkWeek(ukDateTime);
  }

  return ukDate.endOf('week');
};

//build a dayjs with the correct timezone info for the provided day and session start hours and minutes
export const addHoursAndMinutesToUkDatetime = (
  ukDatetime: dayjs.Dayjs,
  hours: number,
  minutes: number,
): dayjs.Dayjs => {
  //have to stringify and parse to ensure the new created datetime has the UK timezone info correct
  //as simply doing .add() in daysjs maintains the original timezone info (even if the operation crosses a DST)
  const newHour = addToUkDatetime(ukDatetime, hours, 'hour', dateTimeFormat);
  return addToUkDatetime(newHour, minutes, 'minute', dateTimeFormat);
};
