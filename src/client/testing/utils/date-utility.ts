import dayjs from 'dayjs';

interface DateComponents {
  day: string;
  month: string;
  year: string;
}

export const getDateInFuture = (
  numberOfDaysFromToday: number,
): DateComponents => {
  const futureDate = dayjs().add(numberOfDaysFromToday, 'day');

  return {
    day: futureDate.format('DD'),
    month: futureDate.format('MM'),
    year: futureDate.format('YYYY'),
  };
};
