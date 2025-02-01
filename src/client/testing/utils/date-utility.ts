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
  dayType: 'Tommorow',
  requiredformat: string,
) => {
  const date = new Date();
  let requiredDate = '';
  if (dayType == 'Tommorow') {
    const tomorrowDate = date.setDate(date.getDate() + 1);
    requiredDate = dayjs(tomorrowDate).format(requiredformat);
  }
  return requiredDate;
};

export const getWeekRange = () => {
  const date = dayjs().add(1, 'day').format('YYYY-MM-DD');

  const startOfWeek = dayjs(date).startOf('isoWeek');
  const endOfWeek = dayjs(date).endOf('isoWeek');
  const formattedStartOfWeek = startOfWeek.format('D MMMM');
  const formattedendOfWeek = endOfWeek.format('D MMMM');
  return `${formattedStartOfWeek} to ${formattedendOfWeek}`;
};
