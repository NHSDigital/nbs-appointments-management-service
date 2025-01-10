import {
  getWeek,
  isBeforeOrEqual,
  toTimeComponents,
} from '@services/timeService';
import {
  fetchDailyAvailability,
  fetchBookings,
} from '@services/appointmentsService';
import dayjs from 'dayjs';
import {
  AvailabilitySession,
  AvailabilitySlot,
  Booking,
  DailyAvailability,
  DaySummary,
  SessionSummary,
} from '@types';

export const summariseWeek = async (
  weekStart: dayjs.Dayjs,
  weekEnd: dayjs.Dayjs,
  siteId: string,
): Promise<DaySummary[]> => {
  const dailyAvailability = await fetchDailyAvailability(
    siteId,
    weekStart.format('YYYY-MM-DD'),
    weekEnd.format('YYYY-MM-DD'),
  );

  const dailyBookings = await fetchBookings({
    from: weekStart.format('YYYY-MM-DD HH:mm:ss'),
    to: weekEnd.format('YYYY-MM-DD HH:mm:ss'),
    site: siteId,
  });

  const week = getWeek(weekStart);

  const daySummaries: DaySummary[] = week.map(day => {
    const availability = dailyAvailability.find(a => dayjs(a.date).isSame(day));
    if (availability === undefined) {
      return {
        date: day,
        sessions: [],
        maximumCapacity: 0,
        bookedAppointments: 0,
        remainingCapacity: 0,
      };
    }

    const bookings = dailyBookings.filter(booking =>
      dayjs(booking.from).isSame(day, 'date'),
    );

    return summariseDay(day, bookings, availability);
  });

  return daySummaries;
};

const summariseDay = (
  date: dayjs.Dayjs,
  bookings: Booking[],
  availability: DailyAvailability,
): DaySummary => {
  let liveBookings = bookings.filter(booking => booking.status === 'Booked');

  const sessionSummaries: SessionSummary[] = availability.sessions.map(
    session => {
      const start = toTimeComponents(session.from);
      const end = toTimeComponents(session.until);

      const startTime = date
        .hour(Number(start?.hour))
        .minute(Number(start?.minute));
      const endTime = date.hour(Number(end?.hour)).minute(Number(end?.minute));

      const slots = divideSessionIntoSlots(startTime, endTime, session);

      const maximumCapacity = slots.length * session.capacity;

      const bookingsByService: Record<string, Booking[]> = {};
      let totalBookings = 0;
      liveBookings.forEach(booking => {
        const matchingSlot = slots.find(
          slot =>
            slot.capacity > 0 &&
            slot.from.isSame(dayjs(booking.from)) &&
            slot.length === booking.duration &&
            slot.services.includes(booking.service),
        );

        if (matchingSlot) {
          bookingsByService[booking.service] = bookingsByService[
            booking.service
          ]
            ? bookingsByService[booking.service].concat(booking)
            : [booking];
          totalBookings += 1;
          liveBookings = liveBookings.splice(liveBookings.indexOf(booking), 1);

          matchingSlot.capacity -= 1;
          slots[slots.indexOf(matchingSlot)] = matchingSlot;
        }
      });

      return {
        start: startTime,
        end: endTime,
        maximumCapacity,
        totalBookings,
        bookings: bookingsByService,
      };
    },
  );

  const maximumCapacity = sessionSummaries.reduce(
    (accumulator, sessionSummary) =>
      accumulator + sessionSummary.maximumCapacity,
    0,
  );

  const bookedAppointments = sessionSummaries.reduce(
    (accumulator, sessionSummary) => accumulator + sessionSummary.totalBookings,
    0,
  );

  const remainingCapacity = maximumCapacity - bookedAppointments;

  return {
    date: date,
    sessions: sessionSummaries,
    maximumCapacity,
    bookedAppointments,
    remainingCapacity,
  };
};

const divideSessionIntoSlots = (
  startTime: dayjs.Dayjs,
  endTime: dayjs.Dayjs,
  session: AvailabilitySession,
): AvailabilitySlot[] => {
  const slots: AvailabilitySlot[] = [];

  let currentSlot = startTime.clone();
  while (
    isBeforeOrEqual(currentSlot, endTime.add(session.slotLength * -1, 'minute'))
  ) {
    slots.push({
      from: currentSlot,
      services: session.services,
      length: session.slotLength,
      capacity: session.capacity,
    });
    currentSlot = currentSlot.add(session.slotLength, 'minute');
  }

  return slots;
};
