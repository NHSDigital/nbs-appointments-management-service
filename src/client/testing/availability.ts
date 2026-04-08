/* eslint-disable @typescript-eslint/no-non-null-assertion */

import {
  DayJsType,
  getWeek,
  isBeforeCalendarDateUk,
  parseDateComponentsToUkDatetime,
  RFC3339Format,
  ukNow,
} from '@services/timeService';
import { AvailabilitySetup, BookingSetup } from './fixtures-v2';

type ServiceOverview = {
  serviceName: string;
  bookedAppointments: number;
};

export type WeekOverview = {
  header: string;
  sessions: ServiceOverview[];
  totalAppointments: number;
  booked: number;
  unbooked: number;
};

export type DayOverview = {
  header: string;
  sessions: DaySessionOverview[];
  totalAppointments: number;
  booked: number;
  unbooked: number;
  orphaned: number;
};

export type DaySessionOverview = {
  sessionTimeInterval: string;
  serviceName: string;
  booked: string;
  unbooked: number;
};

export type RemovedServicesOverview = {
  date: string;
  sessionTimeInterval: string;
  serviceNames: string;
};

type DayTestCase = {
  week: string;
  day: string;
  dayCardHeader: string;
  changeSessionHeader: string;
  cancelSessionHeader: string;
  timeRange: string;
  startHour: string;
  startMins: string;
  endHour: string;
  endMins: string;
  service: string;
  booked: number;
  unbooked: number;
  //the contents of these two SHOULD be identical, but there seems to be some indiscrepancies over these pages...
  viewDailyAppointments: ViewDailyAppointment[];
  cancelDailyAppointments: CancelDailyAppointment[];
};

export type WeekViewTestCase = {
  week: string;
  weekHeader: string;
  previousWeek: string;
  nextWeek: string;
  dayOverviews: DayOverview[];
};

type ViewDailyAppointment = {
  time: string;
  nameNhsNumber: string;
  dob: string;
  contactDetails: string;
  services: string;
};

type CancelDailyAppointment = {
  time: string;
  name: string;
  nhsNumber: string;
  dob: string;
  contactDetails: string;
  services: string;
};

export const timeZones = ['Europe/London', 'Asia/Kamchatka', 'US/Pacific'];

//target data for test across a 3 week period spanning the end of March,
//this guarantees DST boundary is crossed with some data before, on, and after the boundary
export const clockForwardWeeksData = (
  year: number,
): {
  bookings?: BookingSetup[];
  availability?: AvailabilitySetup[];
  firstDate: DayJsType;
  lastDate: DayJsType;
  weekTestCases: WeekViewTestCase[];
} => {
  const twentyFourMarch = parseDateComponentsToUkDatetime({
    day: 24,
    month: 3,
    year: year,
  });
  const thirtyOneMarch = parseDateComponentsToUkDatetime({
    day: 31,
    month: 3,
    year: year,
  });
  const sevenApril = parseDateComponentsToUkDatetime({
    day: 7,
    month: 4,
    year: year,
  });

  const weekOne = getWeek(twentyFourMarch!);
  const weekTwo = getWeek(thirtyOneMarch!);
  const weekThree = getWeek(sevenApril!);

  const allWeeks = weekOne.concat(weekTwo).concat(weekThree);
  const firstBookings = mapToFirstBookings(allWeeks);
  const lastBookings = mapToLastBookings(allWeeks);

  return {
    availability: mapToSessions(allWeeks),
    bookings: firstBookings.concat(lastBookings),
    firstDate: twentyFourMarch!,
    lastDate: sevenApril!,
    weekTestCases: mapToWeekTestCases(weekOne)
      .concat(mapToWeekTestCases(weekTwo))
      .concat(mapToWeekTestCases(weekThree)),
  };
};

export const clockForwardDaysData = (): {
  bookings?: BookingSetup[];
  availability?: AvailabilitySetup[];
  dayTestCases: DayTestCase[];
} => {
  const now = ukNow();

  //guaranteed before DST
  let twentyThreeMarch = parseDateComponentsToUkDatetime({
    day: 24,
    month: 3,
    year: now.year(),
  });

  const isBefore1 = isBeforeCalendarDateUk(now, twentyThreeMarch!);

  //need to set target date to next year to keep within the allowed bounds
  if (!isBefore1) {
    twentyThreeMarch = parseDateComponentsToUkDatetime({
      day: 23,
      month: 3,
      year: now.year() + 1,
    });
  }

  //guaranteed after DST
  let firstApril = parseDateComponentsToUkDatetime({
    day: 1,
    month: 4,
    year: now.year(),
  });

  const isBefore2 = isBeforeCalendarDateUk(now, firstApril!);

  //need to set target date to next year to keep within the allowed bounds
  if (!isBefore2) {
    firstApril = parseDateComponentsToUkDatetime({
      day: 1,
      month: 4,
      year: now.year() + 1,
    });
  }

  const days = [twentyThreeMarch!, firstApril!];

  const firstBookings = mapToFirstBookings(days);
  const lastBookings = mapToLastBookings(days);

  return {
    availability: mapToSessions(days),
    bookings: firstBookings.concat(lastBookings),
    dayTestCases: mapToDayTestCases(days),
  };
};

//target data for test across a 3 week period spanning the end of October,
//this guarantees DST boundary is crossed with some data before, on, and after the boundary
export const clockBackwardWeeksData = (
  year: number,
): {
  bookings?: BookingSetup[];
  availability?: AvailabilitySetup[];
  firstDate: DayJsType;
  lastDate: DayJsType;
  weekTestCases: WeekViewTestCase[];
} => {
  const twentyFourOctober = parseDateComponentsToUkDatetime({
    day: 24,
    month: 10,
    year: year,
  });
  const thirtyOneOctober = parseDateComponentsToUkDatetime({
    day: 31,
    month: 10,
    year: year,
  });
  const sevenNovember = parseDateComponentsToUkDatetime({
    day: 7,
    month: 11,
    year: year,
  });

  const weekOne = getWeek(twentyFourOctober!);
  const weekTwo = getWeek(thirtyOneOctober!);
  const weekThree = getWeek(sevenNovember!);

  const allWeeks = weekOne.concat(weekTwo).concat(weekThree);
  const firstBookings = mapToFirstBookings(allWeeks);
  const lastBookings = mapToLastBookings(allWeeks);

  return {
    availability: mapToSessions(allWeeks),
    bookings: firstBookings.concat(lastBookings),
    firstDate: twentyFourOctober!,
    lastDate: sevenNovember!,
    weekTestCases: mapToWeekTestCases(weekOne)
      .concat(mapToWeekTestCases(weekTwo))
      .concat(mapToWeekTestCases(weekThree)),
  };
};

export const clockBackwardDaysData = (): {
  bookings?: BookingSetup[];
  availability?: AvailabilitySetup[];
  dayTestCases: DayTestCase[];
} => {
  const now = ukNow();

  //guaranteed before DST
  let twentyThreeOctober = parseDateComponentsToUkDatetime({
    day: 24,
    month: 10,
    year: now.year(),
  });

  const isBefore1 = isBeforeCalendarDateUk(now, twentyThreeOctober!);

  //need to set target date to next year to keep within the allowed bounds
  if (!isBefore1) {
    twentyThreeOctober = parseDateComponentsToUkDatetime({
      day: 23,
      month: 10,
      year: now.year() + 1,
    });
  }

  //guaranteed after DST
  let firstNovember = parseDateComponentsToUkDatetime({
    day: 1,
    month: 11,
    year: now.year(),
  });

  const isBefore2 = isBeforeCalendarDateUk(now, firstNovember!);

  //need to set target date to next year to keep within the allowed bounds
  if (!isBefore2) {
    firstNovember = parseDateComponentsToUkDatetime({
      day: 1,
      month: 11,
      year: now.year() + 1,
    });
  }

  const days = [twentyThreeOctober!, firstNovember!];

  const firstBookings = mapToFirstBookings(days);
  const lastBookings = mapToLastBookings(days);

  return {
    availability: mapToSessions(days),
    bookings: firstBookings.concat(lastBookings),
    dayTestCases: mapToDayTestCases(days),
  };
};

const mapToSessions = (days: DayJsType[]): AvailabilitySetup[] => {
  return days.map(day => {
    return {
      date: day.format(RFC3339Format),
      sessions: [
        {
          from: '09:00',
          until: '17:00',
          services: ['RSV Adult'],
          slotLength: 10,
          capacity: 2,
        },
      ],
    } as AvailabilitySetup;
  });
};

const mapToFirstBookings = (days: DayJsType[]): BookingSetup[] => {
  return days.map(day => {
    return {
      fromDate: day.format(RFC3339Format),
      fromTime: '09:00:00',
      durationMins: 10,
      service: 'RSV Adult',
      status: 'Booked',
      availabilityStatus: 'Supported',
      attendeeDetails: {
        nhsNumber: '1975486535-1',
        firstName: 'David',
        lastName: 'Ormond',
        dateOfBirth: '1963-02-03',
      },
    } as BookingSetup;
  });
};

const mapToLastBookings = (days: DayJsType[]): BookingSetup[] => {
  return days.map(day => {
    return {
      fromDate: day.format(RFC3339Format),
      fromTime: '16:50:00',
      durationMins: 10,
      service: 'RSV Adult',
      status: 'Booked',
      availabilityStatus: 'Supported',
      attendeeDetails: {
        nhsNumber: '1975486535-2',
        firstName: 'James',
        lastName: 'Livingstone',
        dateOfBirth: '1949-10-17',
      },
    } as BookingSetup;
  });
};

const mapToWeekTestCases = (week: DayJsType[]): WeekViewTestCase[] => {
  const dayOverviews = [] as DayOverview[];

  for (let i = 0; i < 7; i++) {
    dayOverviews.push({
      header: week[i].format('dddd D MMMM'),
      sessions: [
        {
          serviceName: 'RSV Adult',
          booked: '2 booked',
          unbooked: 94,
          sessionTimeInterval: '09:00 - 17:00',
        },
      ],
      totalAppointments: 96,
      booked: 2,
      unbooked: 94,
      orphaned: 0,
    });
  }

  return [
    {
      week: week[0].format(RFC3339Format),
      weekHeader: `${week[0].format('D MMMM')} to ${week[6].format('D MMMM')}`,
      //TODO add back in
      // previousWeek: '16-22 March 2026',
      // nextWeek: '30 March-5 April 2026',
      dayOverviews: dayOverviews,
    },
  ] as WeekViewTestCase[];
};

const mapToDayTestCases = (days: DayJsType[]): DayTestCase[] => {
  return days.map(day => {
    return {
      day: day.format(RFC3339Format),
      dayCardHeader: day.format('dddd D MMMM'),
      changeSessionHeader: day.format('DD MMMM YYYY'),
      cancelSessionHeader: day.format('dddd DD MMMM'),
      timeRange: '09:00 - 17:00',
      startHour: '09',
      startMins: '00',
      endHour: '17',
      endMins: '00',
      service: 'RSV Adult',
      booked: 2,
      unbooked: 94,

      viewDailyAppointments: [
        {
          time: '09:00',
          nameNhsNumber: 'David Ormond1975486535-1',
          dob: '3 February 1963',
          contactDetails: '',
          services: 'RSV Adult',
        },
        {
          time: '16:50',
          nameNhsNumber: 'James Livingstone1975486535-2',
          dob: '17 October 1949',
          contactDetails: '',
          services: 'RSV Adult',
        },
      ],
      cancelDailyAppointments: [
        {
          time: day.format('D MMMM YYYY') + '9:00am',
          name: 'David Ormond',
          nhsNumber: '1975486535-1',
          dob: '3 February 1963',
          contactDetails: 'Not provided',
          services: 'RSV Adult',
        },
        {
          time: day.format('D MMMM YYYY') + '16:50pm',
          name: 'James Livingstone',
          nhsNumber: '1975486535-2',
          dob: '17 October 1949',
          contactDetails: 'Not provided',
          services: 'RSV Adult',
        },
      ],
    } as DayTestCase;
  });
};
