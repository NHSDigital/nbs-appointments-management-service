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

  // const potentialDate = dayjs(
  //   `${parsedYear}-${parsedMonth}-${parsedDay}`,
  //   'YYYY-M-D',
  //   'en-Gb',
  //   true,
  // );

  const exampleFromDocs = dayjs(
    '35/22/2010 99:88:77',
    'DD-MM-YYYY HH:mm:ss',
    true,
  );

  const exampleWithSingleDayAndMonthDigitFormat = dayjs(
    '35/12/2010 99:88:77',
    'D-M-YYYY HH:mm:ss',
    true,
  );

  const dateStringSlashes = `${parsedDay}/${parsedMonth}/${parsedYear} 00:00:00`;
  const dateStringDashes = `${parsedDay}/${parsedMonth}/${parsedYear} 00:00:00`;

  const potentialDateOne = dayjs(dateStringSlashes, 'D-M-YYYY HH:mm:ss', true);
  const potentialDateTwo = dayjs(dateStringDashes, 'D-M-YYYY HH:mm:ss', true);

  const isOneValid = potentialDateOne.isValid();
  const isTwoValid = potentialDateOne.isValid();

  // Why the HELL doesn't this work?
  return isTwoValid;
};

export const parseDateFromComponents = (
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
    return undefined;
  }

  return dayjs(
    `${parsedDay}-${parsedMonth}-${parsedYear}`,
    'D-M-YYYY',
    'en-Gb',
    true,
  );
};

export const isSameDayOrBefore = (
  firstDate: dayjs.Dayjs,
  secondDate: dayjs.Dayjs,
) => {
  return (
    firstDate.isSame(secondDate, 'day') || firstDate.isBefore(secondDate, 'day')
  );
};
