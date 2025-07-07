import {
  dateTimeFormat,
  dateFormat,
  parseToUkDatetime,
} from '@services/timeService';
import {
  Booking,
  DailyAvailability,
  DaySummary,
  DaySummaryV2,
  WeekSummary,
  WeekSummaryV2,
} from '@types';

const mondayThe10thOfJune2024 = parseToUkDatetime(
  '2024-06-10T00:00:00',
  dateTimeFormat,
);
const sundayThe16thOfJune2024 = parseToUkDatetime(
  '2024-06-16T00:00:00',
  dateTimeFormat,
);

/**
 * A mock week of availability as we'd expect it to be returned from the API.
 * The expected mapping of this is found in @mockWeekAvailability__Summary below.
 * Do not modify one of these without updating the other.
 */
const mockWeekAvailability: DailyAvailability[] = [
  {
    date: mondayThe10thOfJune2024.format(dateFormat),
    sessions: [
      {
        capacity: 2,
        from: '09:00',
        until: '12:00',
        slotLength: 5,
        services: ['RSV:Adult'],
      },
      {
        capacity: 1,
        from: '13:00',
        until: '17:30',
        slotLength: 5,
        services: ['RSV:Adult'],
      },
    ],
  },
  {
    date: mondayThe10thOfJune2024.add(1, 'days').format(dateFormat),
    sessions: [
      {
        capacity: 2,
        from: '09:00',
        until: '12:00',
        slotLength: 10,
        services: ['RSV:Adult', 'FLU:18_64'],
      },
      {
        capacity: 2,
        from: '09:00',
        until: '12:00',
        slotLength: 10,
        services: ['RSV:Adult'],
      },
    ],
  },
  {
    date: mondayThe10thOfJune2024.add(2, 'days').format(dateFormat),
    sessions: [
      {
        capacity: 4,
        from: '08:00',
        until: '12:00',
        slotLength: 10,
        services: ['RSV:Adult', 'FLU:18_64'],
      },
    ],
  },
  {
    date: mondayThe10thOfJune2024.add(3, 'days').format(dateFormat),
    sessions: [
      {
        capacity: 2,
        from: '10:00',
        until: '14:00',
        slotLength: 5,
        services: ['RSV:Adult'],
      },
      {
        capacity: 1,
        from: '15:00',
        until: '18:00',
        slotLength: 10,
        services: ['FLU:18_64'],
      },
    ],
  },
  {
    date: mondayThe10thOfJune2024.add(4, 'days').format(dateFormat),
    sessions: [
      {
        capacity: 3,
        from: '09:00',
        until: '13:00',
        slotLength: 10,
        services: ['RSV:Adult', 'FLU:18_64'],
      },
    ],
  },
  {
    date: mondayThe10thOfJune2024.add(5, 'days').format(dateFormat),
    sessions: [],
  },
  {
    date: mondayThe10thOfJune2024.add(6, 'days').format(dateFormat),
    sessions: [],
  },
];

const mockBooking1: Booking = {
  reference: 'mock-booking-1',
  from: '2024-06-10T09:00:00',
  duration: 5,
  service: 'RSV:Adult',
  site: 'TEST01',
  attendeeDetails: {
    nhsNumber: '9999999990',
    firstName: 'John',
    lastName: 'Smith',
    dateOfBirth: new Date(1979, 1, 1),
  },
  created: '2024-01-04T10:35:08.0477062',
  status: 'Booked',
  availabilityStatus: 'Supported',
  reminderSet: false,
};

const mockBooking2: Booking = {
  reference: 'mock-booking-2',
  from: '2024-06-10T09:00:00',
  duration: 5,
  service: 'RSV:Adult',
  site: 'TEST01',
  attendeeDetails: {
    nhsNumber: '9999999991',
    firstName: 'Sarah',
    lastName: 'Smith',
    dateOfBirth: new Date(1945, 1, 1),
  },
  created: '2024-01-05T10:35:08.0477062',
  status: 'Booked',
  availabilityStatus: 'Supported',
  reminderSet: false,
};

const mockBooking3: Booking = {
  reference: 'mock-booking-3',
  from: '2024-06-10T09:05:00',
  duration: 5,
  service: 'RSV:Adult',
  site: 'TEST01',
  attendeeDetails: {
    nhsNumber: '9999999995',
    firstName: 'Brian',
    lastName: 'Smith',
    dateOfBirth: new Date(1984, 1, 1),
  },
  created: '2024-01-06T09:23:41.0477062',
  status: 'Booked',
  availabilityStatus: 'Orphaned',
  reminderSet: false,
};

const mockBooking4: Booking = {
  reference: 'mock-booking-4',
  from: '2024-06-11T09:10:00',
  duration: 10,
  service: 'FLU:18_64',
  site: 'TEST01',
  attendeeDetails: {
    nhsNumber: '9999999995',
    firstName: 'Brian',
    lastName: 'Smith',
    dateOfBirth: new Date(1984, 1, 1),
  },
  created: '2024-11-05T10:35:08.0477062',
  status: 'Booked',
  availabilityStatus: 'Supported',
  reminderSet: false,
};

const mockBooking5: Booking = {
  reference: 'mock-booking-5',
  from: '2024-06-11T09:10:00',
  duration: 10,
  service: 'RSV:Adult',
  site: 'TEST01',
  attendeeDetails: {
    nhsNumber: '9999999995',
    firstName: 'Ian',
    lastName: 'Goldsmith',
    dateOfBirth: new Date(1973, 2, 3),
  },
  created: '2024-08-29T03:21:08.0477062',
  status: 'Booked',
  availabilityStatus: 'Supported',
  reminderSet: false,
};

const mockBooking6: Booking = {
  reference: 'mock-booking-6',
  from: '2024-06-11T09:10:00',
  duration: 10,
  service: 'RSV:Adult',
  site: 'TEST01',
  attendeeDetails: {
    nhsNumber: '9999999994',
    firstName: 'Zack',
    lastName: 'Jeremiah',
    dateOfBirth: new Date(1973, 2, 3),
  },
  created: '2024-08-29T03:21:08.0477062',
  status: 'Cancelled',
  availabilityStatus: 'Unknown',
  reminderSet: false,
};

const mockBookings: Booking[] = [
  mockBooking1,
  mockBooking2,
  mockBooking3,
  mockBooking4,
  mockBooking5,
  mockBooking6,
];

/**
 * The weekly summary we'd expect to be generated from @mockWeekAvailability above.
 * Do not modify one of these without updating the other.
 */
const mockWeekAvailability__Summary: DaySummary[] = [
  {
    ukDate: parseToUkDatetime('2024-06-10'),
    sessions: [
      {
        ukStartDatetime: '2024-06-10T09:00:00',
        ukEndDatetime: '2024-06-10T12:00:00',
        maximumCapacity: 72,
        totalBookings: 2,
        bookings: { 'RSV:Adult': 2 },
        capacity: 2,
        slotLength: 5,
      },
      {
        ukStartDatetime: '2024-06-10T13:00:00',
        ukEndDatetime: '2024-06-10T17:30:00',
        maximumCapacity: 54,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 1,
        slotLength: 5,
      },
    ],
    maximumCapacity: 126,
    bookedAppointments: 2,
    cancelledAppointments: 0,
    orphanedAppointments: 1,
    remainingCapacity: 124,
  },
  {
    ukDate: parseToUkDatetime('2024-06-11'),
    sessions: [
      {
        ukStartDatetime: '2024-06-11T09:00:00',
        ukEndDatetime: '2024-06-11T12:00:00',
        maximumCapacity: 36,
        totalBookings: 2,
        bookings: { 'RSV:Adult': 1, 'FLU:18_64': 1 },
        capacity: 2,
        slotLength: 10,
      },
      {
        ukStartDatetime: '2024-06-11T09:00:00',
        ukEndDatetime: '2024-06-11T12:00:00',
        maximumCapacity: 36,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 2,
        slotLength: 10,
      },
    ],
    maximumCapacity: 72,
    bookedAppointments: 2,
    cancelledAppointments: 1,
    orphanedAppointments: 0,
    remainingCapacity: 70,
  },
  {
    ukDate: parseToUkDatetime('2024-06-12'),
    sessions: [
      {
        ukStartDatetime: '2024-06-12T08:00:00',
        ukEndDatetime: '2024-06-12T12:00:00',
        maximumCapacity: 96,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0, 'FLU:18_64': 0 },
        capacity: 4,
        slotLength: 10,
      },
    ],
    maximumCapacity: 96,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 96,
  },
  {
    ukDate: parseToUkDatetime('2024-06-13'),
    sessions: [
      {
        ukStartDatetime: '2024-06-13T10:00:00',
        ukEndDatetime: '2024-06-13T14:00:00',
        maximumCapacity: 96,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 2,
        slotLength: 5,
      },
      {
        ukStartDatetime: '2024-06-13T15:00:00',
        ukEndDatetime: '2024-06-13T18:00:00',
        maximumCapacity: 18,
        totalBookings: 0,
        bookings: { 'FLU:18_64': 0 },
        capacity: 1,
        slotLength: 10,
      },
    ],
    maximumCapacity: 114,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 114,
  },
  {
    ukDate: parseToUkDatetime('2024-06-14'),
    sessions: [
      {
        ukStartDatetime: '2024-06-14T09:00:00',
        ukEndDatetime: '2024-06-14T13:00:00',
        maximumCapacity: 72,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0, 'FLU:18_64': 0 },
        capacity: 3,
        slotLength: 10,
      },
    ],
    maximumCapacity: 72,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 72,
  },
  {
    ukDate: parseToUkDatetime('2024-06-15'),
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
  {
    ukDate: parseToUkDatetime('2024-06-16'),
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
];

const mockWeekAvailability__Summary__V2: DaySummaryV2[] = [
  {
    date: '2024-06-10',
    sessions: [
      {
        ukStartDatetime: '2024-06-10T09:00:00',
        ukEndDatetime: '2024-06-10T12:00:00',
        maximumCapacity: 72,
        totalBookings: 2,
        bookings: { 'RSV:Adult': 2 },
        capacity: 2,
        slotLength: 5,
      },
      {
        ukStartDatetime: '2024-06-10T13:00:00',
        ukEndDatetime: '2024-06-10T17:30:00',
        maximumCapacity: 54,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 1,
        slotLength: 5,
      },
    ],
    maximumCapacity: 126,
    bookedAppointments: 2,
    cancelledAppointments: 0,
    orphanedAppointments: 1,
    remainingCapacity: 124,
  },
  {
    date: '2024-06-11',
    sessions: [
      {
        ukStartDatetime: '2024-06-11T09:00:00',
        ukEndDatetime: '2024-06-11T12:00:00',
        maximumCapacity: 36,
        totalBookings: 2,
        bookings: { 'RSV:Adult': 1, 'FLU:18_64': 1 },
        capacity: 2,
        slotLength: 10,
      },
      {
        ukStartDatetime: '2024-06-11T09:00:00',
        ukEndDatetime: '2024-06-11T12:00:00',
        maximumCapacity: 36,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 2,
        slotLength: 10,
      },
    ],
    maximumCapacity: 72,
    bookedAppointments: 2,
    cancelledAppointments: 1,
    orphanedAppointments: 0,
    remainingCapacity: 70,
  },
  {
    date: '2024-06-12',
    sessions: [
      {
        ukStartDatetime: '2024-06-12T08:00:00',
        ukEndDatetime: '2024-06-12T12:00:00',
        maximumCapacity: 96,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0, 'FLU:18_64': 0 },
        capacity: 4,
        slotLength: 10,
      },
    ],
    maximumCapacity: 96,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 96,
  },
  {
    date: '2024-06-13',
    sessions: [
      {
        ukStartDatetime: '2024-06-13T10:00:00',
        ukEndDatetime: '2024-06-13T14:00:00',
        maximumCapacity: 96,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 2,
        slotLength: 5,
      },
      {
        ukStartDatetime: '2024-06-13T15:00:00',
        ukEndDatetime: '2024-06-13T18:00:00',
        maximumCapacity: 18,
        totalBookings: 0,
        bookings: { 'FLU:18_64': 0 },
        capacity: 1,
        slotLength: 10,
      },
    ],
    maximumCapacity: 114,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 114,
  },
  {
    date: '2024-06-14',
    sessions: [
      {
        ukStartDatetime: '2024-06-14T09:00:00',
        ukEndDatetime: '2024-06-14T13:00:00',
        maximumCapacity: 72,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0, 'FLU:18_64': 0 },
        capacity: 3,
        slotLength: 10,
      },
    ],
    maximumCapacity: 72,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 72,
  },
  {
    date: '2024-06-15',
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
  {
    date: '2024-06-16',
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
];

const mockWeekSummary: WeekSummary = {
  startDate: mondayThe10thOfJune2024,
  endDate: sundayThe16thOfJune2024,
  daySummaries: mockWeekAvailability__Summary,
  maximumCapacity: 480,
  bookedAppointments: 4,
  orphanedAppointments: 1,
  remainingCapacity: 476,
};

const mockWeekSummaryV2: WeekSummaryV2 = {
  daySummaries: mockWeekAvailability__Summary__V2,
  maximumCapacity: 480,
  bookedAppointments: 4,
  orphanedAppointments: 1,
  remainingCapacity: 476,
};

export {
  mockBookings,
  mockWeekAvailability,
  mockWeekAvailability__Summary,
  mondayThe10thOfJune2024,
  sundayThe16thOfJune2024,
  mockWeekSummary,
  mockWeekSummaryV2,
};
