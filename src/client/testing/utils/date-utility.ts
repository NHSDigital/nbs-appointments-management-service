import dayjs from 'dayjs';
import isoWeek from 'dayjs/plugin/isoWeek';
dayjs.extend(isoWeek);

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

export const geRequiredtDateInFormat = (
  numberOfDaysFromToday: number,
  requiredformat: string,
) => {
  const date = new Date();
  let requiredDate = '';
  const futureDate = date.setDate(date.getDate() + numberOfDaysFromToday);
  requiredDate = dayjs(futureDate).format(requiredformat);
  return requiredDate;
};

export const getWeekRange = (numberOfDaysFromToday: number) => {
  const date = dayjs().add(numberOfDaysFromToday, 'day').format('YYYY-MM-DD');
  const startOfWeek = dayjs(date).startOf('isoWeek');
  const endOfWeek = dayjs(date).endOf('isoWeek');
  const formattedStartOfWeek = startOfWeek.format('D MMMM');
  const formattedendOfWeek = endOfWeek.format('D MMMM');
  return `${formattedStartOfWeek} to ${formattedendOfWeek}`;
};

export const getWeekRangeForRequiredDate = (requiredDate: string) => {
  const date = dayjs(requiredDate).format('YYYY-MM-DD');
  const startOfWeek = dayjs(date).startOf('isoWeek');
  const endOfWeek = dayjs(date).endOf('isoWeek');
  const formattedStartOfWeek = startOfWeek.format('D MMMM');
  const formattedendOfWeek = endOfWeek.format('D MMMM');
  return `${formattedStartOfWeek} to ${formattedendOfWeek}`;
};
