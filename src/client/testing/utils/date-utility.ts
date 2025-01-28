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
//   const date=new Date();
//   var requiredDate;
//   if(dayType=='Tommorow'){
//     let tomorrowDate = date.setDate(date.getDate()+1);
//     requiredDate=dayjs(tomorrowDate).format(requiredformat);
//     console.log(requiredDate);
//   }
//   return requiredDate;
// };
