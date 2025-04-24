import {
  addToUkDatetime,
  addHoursAndMinutesToUkDatetime,
  dateTimeFormat,
  DayJsType,
  dateFormat,
  getWeek,
  isBefore,
  isBeforeOrEqual,
  isEqual,
  isOnTheSameUkDay,
  parseToUkDatetime,
  parseToTimeComponents,
} from '@services/timeService';
import {
  fetchDailyAvailability,
  fetchBookings,
} from '@services/appointmentsService';
import {
  AvailabilitySession,
  AvailabilitySlot,
  Booking,
  DailyAvailability,
  DaySummary,
  SessionSummary,
  TimeComponents,
  WeekSummary,
} from '@types';

export const summariseWeek = async (
  ukWeekStart: DayJsType,
  ukWeekEnd: DayJsType,
  siteId: string,
): Promise<WeekSummary> => {
  const [dailyAvailability, dailyBookings] = await Promise.all([
    fetchDailyAvailability(
      siteId,
      ukWeekStart.format(dateFormat),
      ukWeekEnd.format(dateFormat),
    ),
    fetchBookings({
      from: ukWeekStart.format('YYYY-MM-DD HH:mm:ss'),
      to: ukWeekEnd.endOf('day').format('YYYY-MM-DD HH:mm:ss'),
      site: siteId,
    }),
  ]);

  const ukWeek = getWeek(ukWeekStart);

  const daySummaries: DaySummary[] = ukWeek.map(ukDate => {
    const availability = dailyAvailability.find(a =>
      isEqual(parseToUkDatetime(a.date), ukDate),
    );

    const bookings = dailyBookings.filter(booking => {
      //need to parse booking datetime back to UK date
      const ukBookingDatetime = parseToUkDatetime(booking.from, dateTimeFormat);
      const result = isOnTheSameUkDay(ukBookingDatetime, ukDate);
      return result;
    });

    return summariseDay(ukDate, bookings, availability);
  });

  const weekSummary = daySummaries.reduce(
    (accumulator, daySummary) => {
      return {
        ...accumulator,
        maximumCapacity:
          accumulator.maximumCapacity + daySummary.maximumCapacity,
        bookedAppointments:
          accumulator.bookedAppointments + daySummary.bookedAppointments,
        orphanedAppointments:
          accumulator.orphanedAppointments + daySummary.orphanedAppointments,
        cancelledAppointments:
          accumulator.cancelledAppointments + daySummary.cancelledAppointments,
        remainingCapacity:
          accumulator.remainingCapacity + daySummary.remainingCapacity,
      };
    },
    {
      startDate: ukWeekStart,
      endDate: ukWeekEnd,
      daySummaries: daySummaries,
      maximumCapacity: 0,
      bookedAppointments: 0,
      orphanedAppointments: 0,
      cancelledAppointments: 0,
      remainingCapacity: 0,
    },
  );

  return weekSummary;
};

const summariseDay = (
  ukDate: DayJsType,
  bookings: Booking[],
  availability?: DailyAvailability,
): DaySummary => {
  const sessionsAndSlots = mapSessionsAndSlots(
    ukDate,
    availability?.sessions ?? [],
  );

  const slots = sessionsAndSlots
    .map(sessionAndSlot => sessionAndSlot.slots)
    .flat();

  let cancelledAppointments = 0;
  let orphanedAppointments = 0;

  bookings.forEach(booking => {
    if (booking.status === 'Cancelled') {
      cancelledAppointments += 1;
      return;
    }

    if (booking.availabilityStatus === 'Orphaned') {
      orphanedAppointments += 1;
      return;
    }

    const matchingSlot = slots.find(slot => {
      const bookingUkDatetime = parseToUkDatetime(booking.from, dateTimeFormat);

      return (
        slot.capacity > 0 &&
        isEqual(slot.from, bookingUkDatetime) &&
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

  return buildDaySummary(
    ukDate,
    sessionsAndSlots,
    cancelledAppointments,
    orphanedAppointments,
  );
};

const buildDaySummary = (
  ukDate: DayJsType,
  sessionsAndSlots: SessionAndSlots[],
  cancelledAppointments: number,
  orphanedAppointments: number,
): DaySummary => {
  const sessionSummaries = sessionsAndSlots
    .map(sessionAndSlot => sessionAndSlot.session)
    .sort((a, b) => {
      const aStart = parseToUkDatetime(a.ukStartDatetime, dateTimeFormat);
      const bStart = parseToUkDatetime(b.ukStartDatetime, dateTimeFormat);
      const aEnd = parseToUkDatetime(a.ukEndDatetime, dateTimeFormat);
      const bEnd = parseToUkDatetime(b.ukEndDatetime, dateTimeFormat);

      if (isBefore(aStart, bStart)) {
        return -1;
      }
      if (isBefore(aEnd, bEnd)) {
        return -1;
      }
      if (a.bookings > b.bookings) {
        return -1;
      }
      return 0;
    });

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
    ukDate,
    sessions: sessionSummaries,
    maximumCapacity,
    bookedAppointments,
    remainingCapacity,
    cancelledAppointments,
    orphanedAppointments,
  };
};

export const divideSessionIntoSlots = (
  sessionIndex: number,
  ukStartDatetime: DayJsType,
  ukEndDatetime: DayJsType,
  session: AvailabilitySession,
): AvailabilitySlot[] => {
  const slots: AvailabilitySlot[] = [];

  let currentSlot = ukStartDatetime.clone();
  const bookEnd = addToUkDatetime(
    ukEndDatetime,
    session.slotLength * -1,
    'minute',
    dateTimeFormat,
  );
  while (isBeforeOrEqual(currentSlot, bookEnd)) {
    slots.push({
      sessionIndex,
      from: currentSlot,
      services: session.services,
      length: session.slotLength,
      capacity: session.capacity,
    });

    currentSlot = addToUkDatetime(
      currentSlot,
      session.slotLength,
      'minute',
      dateTimeFormat,
    );
  }

  return slots;
};

const mapSessionsAndSlots = (
  ukDate: DayJsType,
  sessions: AvailabilitySession[],
): SessionAndSlots[] =>
  sessions.map((session, index) => {
    const start = parseToTimeComponents(session.from);
    const end = parseToTimeComponents(session.until);

    const ukStartDatetime = addHoursAndMinutesToUkDatetime(
      ukDate,
      Number(start?.hour),
      Number(start?.minute),
    );

    const ukEndDatetime = addHoursAndMinutesToUkDatetime(
      ukDate,
      Number(end?.hour),
      Number(end?.minute),
    );

    const slotsInSession = divideSessionIntoSlots(
      index,
      ukStartDatetime,
      ukEndDatetime,
      session,
    );

    const bookingsByService: Record<string, number> = {};
    session.services.forEach(service => {
      bookingsByService[service] = 0;
    });

    const sessionSummary: SessionSummary = {
      ukStartDatetime: ukStartDatetime.format(dateTimeFormat),
      ukEndDatetime: ukEndDatetime.format(dateTimeFormat),
      maximumCapacity: slotsInSession.length * session.capacity,
      totalBookings: 0,
      bookings: bookingsByService,
      capacity: session.capacity,
      slotLength: session.slotLength,
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

export const sessionLengthInMinutes = (
  startTime: TimeComponents,
  endTime: TimeComponents,
): number => {
  const startMinutes = Number(startTime.hour) * 60 + Number(startTime.minute);
  const endMinutes = Number(endTime.hour) * 60 + Number(endTime.minute);

  return endMinutes - startMinutes;
};
