import dayjs from 'dayjs';
import isoWeek from 'dayjs/plugin/isoWeek'; // import plugin
import customParseFormat from 'dayjs/plugin/customParseFormat';

dayjs.extend(isoWeek);
dayjs.extend(customParseFormat);

const parsedEnglishDateStringOrToday = (dateString?: string) => {
  if (dayjs(dateString, 'DD-MM-YYYY').isValid()) {
    return dayjs(dateString, 'DD-MM-YYYY');
  }

  return dayjs();
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

const getLastMonthName = (date: dayjs.Dayjs) => {
  return date.subtract(1, 'month').format('MMMM YYYY');
};

const getNextMonthName = (date: dayjs.Dayjs) => {
  return date.add(1, 'month').format('MMMM YYYY');
};

export {
  getWeeksOfTheMonth,
  getLastMonthName,
  getNextMonthName,
  parsedEnglishDateStringOrToday,
};
