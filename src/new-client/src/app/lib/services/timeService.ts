import dayjs from 'dayjs';
import isoWeek from 'dayjs/plugin/isoWeek';
import customParseFormat from 'dayjs/plugin/customParseFormat';

dayjs.extend(isoWeek);
dayjs.extend(customParseFormat);

const parseDate = (dateString?: string) => {
  if (dayjs(dateString, 'DD-MM-YYYY').isValid()) {
    return dayjs(dateString, 'DD-MM-YYYY');
  }

  return dayjs();
};

const formatDateForUrl = (date: dayjs.Dayjs) => {
  return date.format('DD-MM-YYYY');
};

const getWeeksOfTheMonth = (dateInMonth: dayjs.Dayjs) => {
  const startOfWeek = dateInMonth.startOf('month').startOf('isoWeek');
  const endOfMonth = dateInMonth.endOf('month');

  const dates: dayjs.Dayjs[][] = [];
  let currentWeek: dayjs.Dayjs[] = [];
  let currentDate = startOfWeek;

  while (
    currentDate.isBefore(endOfMonth) ||
    currentDate.isSame(endOfMonth, 'day')
  ) {
    currentWeek.push(currentDate);
    if (currentWeek.length === 7) {
      dates.push(currentWeek);
      currentWeek = [];
    }
    currentDate = currentDate.add(1, 'day');
  }

  if (currentWeek.length > 0) {
    dates.push(currentWeek);
  }

  return dates;
};

type Month = {
  month: string;
  weeks: dayjs.Dayjs[][];
};

const getMonthsOfTheYear = (dateInYear: dayjs.Dayjs) => {
  const months: Month[] = [];
  let currentMonth = dateInYear.startOf('year');

  for (let i = 0; i < 12; i++) {
    const weeks = getWeeksOfTheMonth(currentMonth);
    months.push({ month: currentMonth.format('MMMM'), weeks });
    currentMonth = currentMonth.add(1, 'month');
  }

  return months;
};

const getLastMonthName = (date: dayjs.Dayjs) => {
  return date.subtract(1, 'month').format('MMMM YYYY');
};

const getNextMonthName = (date: dayjs.Dayjs) => {
  return date.add(1, 'month').format('MMMM YYYY');
};

const getDaysOfTheWeek = (dateInWeek: dayjs.Dayjs) => {
  const startOfWeek = dateInWeek.startOf('isoWeek');
  const endOfWeek = dateInWeek.endOf('isoWeek');

  const days: dayjs.Dayjs[] = [];
  let currentDate = startOfWeek;

  while (
    currentDate.isBefore(endOfWeek) ||
    currentDate.isSame(endOfWeek, 'day')
  ) {
    days.push(currentDate);
    currentDate = currentDate.add(1, 'day');
  }

  return days;
};

export {
  getWeeksOfTheMonth,
  getMonthsOfTheYear,
  getDaysOfTheWeek,
  getLastMonthName,
  getNextMonthName,
  parseDate,
  formatDateForUrl,
};
export type { Month };
