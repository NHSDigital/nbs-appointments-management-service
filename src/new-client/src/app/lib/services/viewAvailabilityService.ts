import {
  AvailabilityBlock,
  AvailabilityResponse,
  Booking,
  clinicalServices,
  FetchBookingsRequest,
  Week,
} from '@types';
import dayjs from 'dayjs';
import { fetchBookings } from './appointmentsService';

// TODO: Look into optimising these methods as there is a chunk of duplicated code
export const getWeeksInMonth = (year: number, month: number): Week[] => {
  const weeks: Week[] = [];

  const firstDate = new Date(year, month, 1);
  const lastDate = new Date(year, month + 1, 0);
  const numDays = lastDate.getDate();
  const dayOfWeekCounter = firstDate.getDay();

  let start = 1;
  let end = 7;
  if (dayOfWeekCounter === 0) {
    end = 1;
  } else {
    end = 7 - dayOfWeekCounter + 1;
  }

  while (start <= numDays) {
    weeks.push({
      start: start,
      end: end,
      startMonth: month,
      endMonth: month,
      year: year,
      bookedAppointments: [],
    });
    start = end + 1;
    end = end + 7;
    end = start === 1 && end === 8 ? 1 : end;
    if (end > numDays) {
      end = numDays;
    }
  }

  if (weeks[0].start === 1) {
    const beforeIndex = addMonth(year, month - 1, 1);
    weeks[0].start = beforeIndex.start;
    weeks[0].startMonth = beforeIndex.startMonth;
  }

  if (weeks[weeks.length - 1].end === numDays) {
    const afterIndex = addMonth(year, month + 1, 0);
    weeks[weeks.length - 1].end = afterIndex.start;
    weeks[weeks.length - 1].endMonth = afterIndex.endMonth;
  }

  return weeks;
};

const addMonth = (year: number, month: number, flag: number): Week => {
  const weeks: Week[] = [];

  const firstDate = new Date(year, month, 1);
  const lastDate = new Date(year, month + 1, 0);
  const numDays = lastDate.getDate();
  const dayOfWeekCounter = firstDate.getDay();

  let start = 1;
  let end = 7;
  if (dayOfWeekCounter === 0) {
    end = 1;
  } else {
    end = 7 - dayOfWeekCounter + 1;
  }

  while (start <= numDays) {
    weeks.push({
      start: start,
      end: end,
      startMonth: month,
      endMonth: month,
      year: year,
      bookedAppointments: [],
    });
    start = end + 1;
    end = end + 7;
    end = start === 1 && end === 8 ? 1 : end;
    if (end > numDays) {
      end = numDays;
    }
  }

  if (flag === 0) {
    return weeks[0];
  }

  if (flag === 1) {
    return weeks[weeks.length - 1];
  }

  return weeks[0];
};

// TODO: Can this be optimised / cleaned up?
export const getUnbookedCount = (
  availability: AvailabilityResponse[],
  week: Week,
): number => {
  let unbookedCount = 0;
  if (!availability || availability.length === 0) {
    return unbookedCount;
  }

  for (let a = 0; a < availability.length; a++) {
    if (availability[a].availability.length === 0) {
      continue;
    }

    const fromDate = weekStart(week);
    const toDate = weekEnd(week);

    const blocks: AvailabilityBlock[] = [];
    availability[a].availability.filter(item => {
      const date = new Date(item.date);
      if (
        date.getTime() >= fromDate.getTime() &&
        date.getTime() <= toDate.getTime()
      ) {
        blocks.push(...item.blocks);
      }
    });

    const countArray = blocks.map(b => b.count);
    unbookedCount = countArray.reduce((sum, num) => sum + num);
  }

  return unbookedCount;
};

const weekStart = (week: Week): Date => {
  return new Date(week.year, week.startMonth, week.start, 0, 0, 0);
};
const weekEnd = (week: Week): Date => {
  return new Date(week.year, week.endMonth, week.end, 23, 59, 59);
};

const getBookingsInWeek = (
  bookings: Booking[],
  from: Date,
  to: Date,
): Booking[] => {
  return bookings.filter(b => {
    const bookingDate = new Date(b.from);
    return (
      bookingDate.getTime() >= from.getTime() &&
      bookingDate.getTime() <= to.getTime()
    );
  });
};

export const getDetailedMonthView = async (
  availability: AvailabilityResponse[],
  siteId: string,
): Promise<Week[]> => {
  const weeks = getWeeksInMonth(dayjs().year(), dayjs().month());
  const fromDate = new Date(
    weeks[0].year,
    weeks[0].startMonth,
    weeks[0].start,
    0,
    0,
    0,
  );
  const toDate = new Date(
    weeks[weeks.length - 1].year,
    weeks[weeks.length - 1].endMonth,
    weeks[weeks.length - 1].end,
    23,
    59,
    59,
  );

  const bookingRequest: FetchBookingsRequest = {
    from: dayjs(fromDate).format('YYYY-MM-DD H:mm'),
    to: dayjs(toDate).format('YYYY-MM-DD H:mm'),
    site: siteId,
  };

  const bookings = await fetchBookings(bookingRequest);

  for (let w = 0; w < weeks.length; w++) {
    const week = weeks[w];
    const bookingsInWeek = getBookingsInWeek(
      bookings,
      weekStart(week),
      weekEnd(week),
    );
    week.booked = bookingsInWeek.length;
    week.unbooked = getUnbookedCount(availability, week);
    week.totalAppointments = bookingsInWeek.length + (week.unbooked ?? 0);

    clinicalServices.map(c => {
      const bookedAppts = bookingsInWeek.filter(b => b.service === c.value);
      week.bookedAppointments?.push({
        service: c.label,
        count: bookedAppts.length,
      });
    });
  }

  return weeks;
};
