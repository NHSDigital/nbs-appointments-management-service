import dayjs from 'dayjs';

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

  // TODO: Find a way of doing this without manual calculations. The below *should* work but doesn't
  // const potentialDate = dayjs(
  //   `${parsedYear}-${parsedMonth}-${parsedDay}`,
  //   'YYYY-M-D',
  //   'en-Gb',
  //   true,
  // );

  // return potentialDate.isValid();

  // TODO: This is the worst code I've ever written and I hate it,
  // but I spent 3+ hours trying to do this properly and it just doesn't work
  // but writing it this way took 5 minutes and works perfectly
  if (parsedDay < 1 || parsedDay > 31) {
    return false;
  }
  if (parsedMonth < 1 || parsedMonth > 12) {
    return false;
  }
  if (parsedYear < 1000 || parsedYear > 9999) {
    return false;
  }
  const monthsWith30Days = [4, 6, 9, 11];
  if (monthsWith30Days.includes(parsedMonth) && parsedDay > 30) {
    return false;
  }

  const isLeapYear = parsedYear % 4 === 0;
  if (parsedMonth === 2) {
    if (isLeapYear && parsedDay > 29) {
      return false;
    }
    if (!isLeapYear && parsedDay > 28) {
      return false;
    }
  }
  return true;
};

export const parseAndValidateDateFromComponents = (
  day: string | number,
  month: string | number,
  year: string | number,
) => {
  if (!isValidDate(day, month, year)) {
    return undefined;
  }

  return dayjs(`${day}-${month}-${year}`, 'D-M-YYYY', 'en-Gb', true);
};

export const isSameDayOrBefore = (
  firstDate: dayjs.Dayjs,
  secondDate: dayjs.Dayjs,
) => {
  return (
    firstDate.isSame(secondDate, 'day') || firstDate.isBefore(secondDate, 'day')
  );
};
