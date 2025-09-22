import {
  addToUkDatetime,
  addHoursAndMinutesToUkDatetime,
  dateTimeFormat,
  DayJsType,
  RFC3339Format,
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
  DaySummaryV2,
  SessionSummary,
  TimeComponents,
  WeekSummary,
  WeekSummaryV2,
} from '@types';
import fromServer from '@server/fromServer';

export const summariseWeek = async (
  ukWeekStart: DayJsType,
  ukWeekEnd: DayJsType,
  siteId: string,
): Promise<WeekSummary> => {
  const [dailyAvailability, dailyBookings] = await Promise.all([
    fromServer(
      fetchDailyAvailability(
        siteId,
        ukWeekStart.format(RFC3339Format),
        ukWeekEnd.format(RFC3339Format),
      ),
    ),
    fromServer(
      fetchBookings(
        {
          from: ukWeekStart.format(dateTimeFormat),
          to: ukWeekEnd.endOf('day').format(dateTimeFormat),
          site: siteId,
        },
        ['Booked', 'Cancelled'],
      ),
    ),
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
      remainingCapacity: 0,
    },
  );

  return weekSummary;
};

export const mapWeekSummary = (
  ukWeekStart: DayJsType,
  ukWeekEnd: DayJsType,
  weekSummaryV2: WeekSummaryV2,
): WeekSummary => {
  return {
    ...weekSummaryV2,
    startDate: ukWeekStart,
    endDate: ukWeekEnd,
    remainingCapacity: weekSummaryV2.totalRemainingCapacity,
    bookedAppointments: weekSummaryV2.totalSupportedAppointments,
    orphanedAppointments: weekSummaryV2.totalOrphanedAppointments,
    daySummaries: mapDaySummaries(weekSummaryV2.daySummaries),
  };
};

const mapDaySummaries = (daySummaries: DaySummaryV2[]): DaySummary[] => {
  return daySummaries.map(daySummaryV2 => {
    return {
      ...daySummaryV2,
      sessions: daySummaryV2.sessionSummaries,
      remainingCapacity: daySummaryV2.totalRemainingCapacity,
      bookedAppointments: daySummaryV2.totalSupportedAppointments,
      cancelledAppointments: daySummaryV2.totalCancelledAppointments,
      orphanedAppointments: daySummaryV2.totalOrphanedAppointments,
      ukDate: parseToUkDatetime(daySummaryV2.date),
    };
  });
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
      sessionSlotCameFrom.totalSupportedAppointmentsByService[
        booking.service
      ] += 1;

      // 3. Add the booking to the session's total bookings
      sessionSlotCameFrom.totalSupportedAppointments += 1;
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
      if (a.totalSupportedAppointments > b.totalSupportedAppointments) {
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
    (accumulator, sessionSummary) =>
      accumulator + sessionSummary.totalSupportedAppointments,
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
      totalSupportedAppointments: 0,
      totalSupportedAppointmentsByService: bookingsByService,
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

export function isBookingOrphaned(
  booking: Booking,
  slots: AvailabilitySlot[],
  session: AvailabilitySession,
): boolean {
  const bookingTime = parseToUkDatetime(booking.from, dateTimeFormat);
  const startTime = parseToUkDatetime(session.from, dateTimeFormat);
  const slotLength = session.slotLength;

  // Check if booking aligns with new slot grid
  const offset = bookingTime.diff(startTime, 'minute') % slotLength;
  const isAligned = offset === 0;

  if (!isAligned) return false;

  // Find a matching slot
  const matchingSlot = slots.find(
    slot =>
      slot.capacity > 0 &&
      slot.length === booking.duration &&
      slot.services.includes(booking.service) &&
      slot.from.isSame(bookingTime),
  );

  return !!matchingSlot;
}

export const evaluateSessionChangeImpact = (
  updatedSession: AvailabilitySession,
  bookings: Booking[],
  ukDate: DayJsType,
): {
  orphanedBookings: Booking[];
  orphanedCount: number;
  canShortenWithoutImpact: boolean;
} => {
  const start = parseToTimeComponents(updatedSession.from);
  const end = parseToTimeComponents(updatedSession.until);

  const newStart = addHoursAndMinutesToUkDatetime(
    ukDate,
    Number(start?.hour),
    Number(start?.minute),
  );

  const newEnd = addHoursAndMinutesToUkDatetime(
    ukDate,
    Number(end?.hour),
    Number(end?.minute),
  );

  const newSlots = divideSessionIntoSlots(0, newStart, newEnd, updatedSession);

  const orphaned: Booking[] = [];

  bookings.forEach(booking => {
    const bookingTime = parseToUkDatetime(booking.from, dateTimeFormat);
    const offset =
      bookingTime.diff(newStart, 'minute') % updatedSession.slotLength;
    const isAligned = offset === 0;

    const matchingSlot = newSlots.find(
      slot =>
        isAligned &&
        slot.from.isSame(bookingTime) &&
        slot.length === booking.duration &&
        slot.services.includes(booking.service),
    );

    if (!matchingSlot) {
      orphaned.push(booking);
    }
  });

  return {
    orphanedBookings: orphaned,
    orphanedCount: orphaned.length,
    canShortenWithoutImpact: orphaned.length === 0,
  };
};
