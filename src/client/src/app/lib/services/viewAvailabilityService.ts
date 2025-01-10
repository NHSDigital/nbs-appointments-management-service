import {
  AvailabilityBlock,
  AvailabilityResponse,
  Booking,
  clinicalServices,
  FetchBookingsRequest,
  Week,
} from '@types';
import dayjs, { Dayjs } from 'dayjs';
import { fetchBookings } from './appointmentsService';
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';
import isoWeek from 'dayjs/plugin/isoWeek';

dayjs.extend(isSameOrAfter);
dayjs.extend(isSameOrBefore);
dayjs.extend(isoWeek);

const BuildWeeks = (year: number, month: number): Week[] => {
  const weeks: Week[] = [];
  const first = dayjs().year(year).month(month).startOf('month');
  const last = dayjs().year(year).month(month).endOf('month');
  const numDays = last.daysInMonth();
  const dayOfWeekCounter = first.day();

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
      startYear: year,
      endYear: year,
      bookedAppointments: [],
      endDate: dayjs()
        .year(year)
        .month(month)
        .date(end)
        .hour(23)
        .minute(59)
        .second(59),
      startDate: dayjs()
        .year(year)
        .month(month)
        .date(start)
        .hour(0)
        .minute(0)
        .second(0),
    });
    start = end + 1;
    end = end + 7;
    end = start === 1 && end === 8 ? 1 : end;
    if (end > numDays) {
      end = numDays;
    }
  }

  return weeks;
};

export const getWeeksInMonth = (year: number, month: number): Week[] => {
  const weeks = BuildWeeks(year, month);

  if (weeks[0].start === 1) {
    const beforeWeeks =
      month === 0 ? BuildWeeks(year - 1, 11) : BuildWeeks(year, month - 1);
    const firstWeek = beforeWeeks[beforeWeeks.length - 1];
    weeks[0].startDate = dayjs()
      .year(firstWeek.startYear)
      .month(firstWeek.startMonth)
      .date(firstWeek.start)
      .hour(0)
      .minute(0);
  }

  if (
    weeks[weeks.length - 1].end ===
    dayjs().year(year).month(month).endOf('month').date()
  ) {
    const afterWeeks =
      month === 11 ? BuildWeeks(year + 1, 0) : BuildWeeks(year, month + 1);
    const lastWeek = afterWeeks[0];
    weeks[weeks.length - 1].endDate = dayjs()
      .year(lastWeek.startYear)
      .month(lastWeek.endMonth)
      .date(lastWeek.end)
      .hour(23)
      .minute(59);
  }

  return weeks;
};

const getUnbookedCount = (
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

    const fromDate = week.startDate.startOf('date');
    const toDate = week.endDate.endOf('date');

    const blocks: AvailabilityBlock[] = [];
    availability[a].availability.filter(item => {
      const date = dayjs(item.date);
      if (
        date.startOf('date').isSameOrAfter(fromDate) &&
        date.isSameOrBefore(toDate)
      ) {
        blocks.push(...item.blocks);
      }
    });

    const countArray = blocks.map(b => b.count);
    unbookedCount =
      countArray.length === 0 ? 0 : countArray.reduce((sum, num) => sum + num);
  }

  return unbookedCount;
};

const getBookingsInWeek = (
  bookings: Booking[],
  from: Dayjs,
  to: Dayjs,
): Booking[] => {
  return bookings.filter(b => {
    const bookingDate = dayjs(b.from);
    return bookingDate.isSameOrAfter(from) && bookingDate.isSameOrBefore(to);
  });
};

export const getDetailedMonthView = async (
  availability: AvailabilityResponse[],
  weeks: Week[],
  siteId: string,
): Promise<Week[]> => {
  const firstWeek = weeks[0];
  const lastWeek = weeks[weeks.length - 1];

  const bookingRequest: FetchBookingsRequest = {
    from: firstWeek.startDate.format('YYYY-MM-DD H:mm'),
    to: lastWeek.endDate.format('YYYY-MM-DD H:mm'),
    site: siteId,
  };

  const bookings = await fetchBookings(bookingRequest);

  for (let w = 0; w < weeks.length; w++) {
    const week = weeks[w];
    const bookingsInWeek = getBookingsInWeek(
      bookings,
      week.startDate,
      week.endDate,
    );
    const booked = bookingsInWeek.filter(b => b.status === 'Booked');
    week.booked = booked.length;
    week.unbooked = getUnbookedCount(availability, week);
    week.totalAppointments = booked.length + (week.unbooked ?? 0);

    clinicalServices.map(c => {
      const bookedAppts = booked.filter(b => b.service === c.value);
      week.bookedAppointments?.push({
        service: c.label,
        count: bookedAppts.length,
      });
    });
  }

  return weeks;
};
