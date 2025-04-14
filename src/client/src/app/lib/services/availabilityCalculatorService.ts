import {
  addToUkDate,
  getWeek,
  isBeforeOrEqual,
  isoTimezoneToDayjs,
  parseDateStringToUkDatetime,
  toTimeComponents,
  ukTimezone,
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
  TimeComponents,
  WeekSummary,
} from '@types';

export const summariseWeek = async (
  ukWeekStart: dayjs.Dayjs,
  ukWeekEnd: dayjs.Dayjs,
  siteId: string,
): Promise<WeekSummary> => {
  const [dailyAvailability, dailyBookings] = await Promise.all([
    fetchDailyAvailability(
      siteId,
      ukWeekStart.format('YYYY-MM-DD'),
      ukWeekEnd.format('YYYY-MM-DD'),
    ),
    fetchBookings({
      from: ukWeekStart.format('YYYY-MM-DD HH:mm:ss'),
      to: ukWeekEnd.add(1, 'day').format('YYYY-MM-DD HH:mm:ss'),
      site: siteId,
    }),
  ]);

  const ukWeek = getWeek(ukWeekStart);

  const daySummaries: DaySummary[] = ukWeek.map(ukDay => {
    const availability = dailyAvailability.find(a =>
      parseDateStringToUkDatetime(a.date).isSame(ukDay),
    );

    const bookings = dailyBookings.filter(booking => {
      //need to parse booking datetime back to UK date
      const ukBookingDatetime = parseDateStringToUkDatetime(
        booking.from,
        'YYYY-MM-DDTHH:mm:ss',
      );
      const result = ukBookingDatetime.isSame(ukDay, 'day');
      return result;
    });

    return summariseDay(ukDay, bookings, availability);
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
  ukDate: dayjs.Dayjs,
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
      const bookingUkDatetime = parseDateStringToUkDatetime(
        booking.from,
        'YYYY-MM-DDTHH:mm:ss',
      );

      return (
        slot.capacity > 0 &&
        slot.from.isSame(bookingUkDatetime) &&
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
  ukDate: dayjs.Dayjs,
  sessionsAndSlots: SessionAndSlots[],
  cancelledAppointments: number,
  orphanedAppointments: number,
): DaySummary => {
  const sessionSummaries = sessionsAndSlots
    .map(sessionAndSlot => sessionAndSlot.session)
    .sort((a, b) => {
      const aStart = isoTimezoneToDayjs(a.ukStart);
      const bStart = isoTimezoneToDayjs(b.ukStart);
      const aEnd = isoTimezoneToDayjs(a.ukEnd);
      const bEnd = isoTimezoneToDayjs(b.ukEnd);

      if (aStart.isBefore(bStart)) {
        return -1;
      }
      if (aEnd.isBefore(bEnd)) {
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

const divideSessionIntoSlots = (
  sessionIndex: number,
  ukStartTime: dayjs.Dayjs,
  ukEndTime: dayjs.Dayjs,
  session: AvailabilitySession,
): AvailabilitySlot[] => {
  const slots: AvailabilitySlot[] = [];

  let currentSlot = ukStartTime.clone();
  while (
    isBeforeOrEqual(
      currentSlot,
      addToUkDate(
        ukEndTime,
        session.slotLength * -1,
        'minute',
        'YYYY-MM-DDTHH:mm:ss',
      ),
    )
  ) {
    slots.push({
      sessionIndex,
      from: currentSlot,
      services: session.services,
      length: session.slotLength,
      capacity: session.capacity,
    });

    currentSlot = addToUkDate(
      currentSlot,
      session.slotLength,
      'minute',
      'YYYY-MM-DDTHH:mm:ss',
    );
  }

  return slots;
};

const mapSessionsAndSlots = (
  ukDate: dayjs.Dayjs,
  sessions: AvailabilitySession[],
): SessionAndSlots[] =>
  sessions.map((session, index) => {
    const start = toTimeComponents(session.from);
    const end = toTimeComponents(session.until);

    const ukStartTime = ukDate
      .hour(Number(start?.hour))
      .minute(Number(start?.minute));
    const ukEndTime = ukDate
      .hour(Number(end?.hour))
      .minute(Number(end?.minute));

    const slotsInSession = divideSessionIntoSlots(
      index,
      ukStartTime,
      ukEndTime,
      session,
    );

    const bookingsByService: Record<string, number> = {};
    session.services.forEach(service => {
      bookingsByService[service] = 0;
    });

    const sessionSummary: SessionSummary = {
      ukStart: {
        iso: ukStartTime.toISOString(),
        tz: ukTimezone,
      },
      ukEnd: {
        iso: ukEndTime.toISOString(),
        tz: ukTimezone,
      },
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
