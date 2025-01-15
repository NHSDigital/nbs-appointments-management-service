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

    const bookings = dailyBookings.filter(
      booking =>
        dayjs(booking.from).isSame(day, 'date') && booking.status === 'Booked',
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
  const sessionsAndSlots = mapSessionsAndSlots(date, availability.sessions);

  const slots = sessionsAndSlots
    .map(sessionAndSlot => sessionAndSlot.slots)
    .flat();

  bookings.forEach(booking => {
    const matchingSlot = slots.find(slot => {
      return (
        slot.capacity > 0 &&
        slot.from.isSame(dayjs(booking.from)) &&
        slot.length === booking.duration &&
        slot.services.includes(booking.service)
      );
    });

    const sessionSlotCameFrom = sessionsAndSlots.find(
      sessionAndSlot =>
        sessionAndSlot.sessionIndex === matchingSlot?.sessionIndex,
    )?.session;

    if (matchingSlot && sessionSlotCameFrom) {
      // 1. Reduce the matching slot's capacity
      matchingSlot.capacity -= 1;

      // 2. Add the booking to the session's bookings
      sessionSlotCameFrom.bookings[booking.service] += 1;

      // 3. Add the booking to the session's total bookings
      sessionSlotCameFrom.totalBookings += 1;
    }
  });

  return buildDaySummary(date, sessionsAndSlots);
};

const buildDaySummary = (
  date: dayjs.Dayjs,
  sessionsAndSlots: SessionAndSlots[],
): DaySummary => {
  const sessionSummaries = sessionsAndSlots.map(
    sessionAndSlot => sessionAndSlot.session,
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
  sessionIndex: number,
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
      sessionIndex,
      from: currentSlot,
      services: session.services,
      length: session.slotLength,
      capacity: session.capacity,
    });
    currentSlot = currentSlot.add(session.slotLength, 'minute');
  }

  return slots;
};

const mapSessionsAndSlots = (
  date: dayjs.Dayjs,
  sessions: AvailabilitySession[],
): SessionAndSlots[] =>
  sessions.map((session, index) => {
    const start = toTimeComponents(session.from);
    const end = toTimeComponents(session.until);

    const startTime = date
      .hour(Number(start?.hour))
      .minute(Number(start?.minute));
    const endTime = date.hour(Number(end?.hour)).minute(Number(end?.minute));

    const slotsInSession = divideSessionIntoSlots(
      index,
      startTime,
      endTime,
      session,
    );

    const bookingsByService: Record<string, number> = {};
    session.services.forEach(service => {
      bookingsByService[service] = 0;
    });

    const sessionSummary: SessionSummary = {
      start: startTime,
      end: endTime,
      maximumCapacity: slotsInSession.length * session.capacity,
      totalBookings: 0,
      bookings: bookingsByService,
    };

    return {
      sessionIndex: index,
      session: sessionSummary,
      slots: slotsInSession,
    };
  });

type SessionAndSlots = {
  sessionIndex: number;
  session: SessionSummary;
  slots: AvailabilitySlot[];
};
