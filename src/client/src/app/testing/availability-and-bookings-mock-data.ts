import { Booking, DailyAvailability, DaySummary, WeekSummary } from '@types';
import dayjs from 'dayjs';

const mondayThe10thOfJune2024 = dayjs('2024-06-10T00:00:00Z');
const sundayThe16thOfJune2024 = dayjs('2024-06-16T00:00:00Z');

/**
 * A mock week of availability as we'd expect it to be returned from the API.
 * The expected mapping of this is found in @mockWeekAvailability__Summary below.
 * Do not modify one of these without updating the other.
 */
const mockWeekAvailability: DailyAvailability[] = [
  {
    date: mondayThe10thOfJune2024.format('YYYY-MM-DD'),
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
    date: mondayThe10thOfJune2024.add(1, 'days').format('YYYY-MM-DD'),
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
    date: mondayThe10thOfJune2024.add(2, 'days').format('YYYY-MM-DD'),
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
    date: mondayThe10thOfJune2024.add(3, 'days').format('YYYY-MM-DD'),
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
    date: mondayThe10thOfJune2024.add(4, 'days').format('YYYY-MM-DD'),
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
    date: mondayThe10thOfJune2024.add(5, 'days').format('YYYY-MM-DD'),
    sessions: [],
  },
  {
    date: mondayThe10thOfJune2024.add(6, 'days').format('YYYY-MM-DD'),
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

const mockBooking7: Booking = {
  reference: 'mock-booking-7',
  from: '2024-06-11T09:20:00',
  duration: 10,
  service: 'RSV:Adult',
  site: 'TEST01',
  attendeeDetails: {
    nhsNumber: '9999999993',
    firstName: 'Bertha',
    lastName: 'Mildrew',
    dateOfBirth: new Date(1973, 2, 3),
  },
  created: '2024-08-29T03:21:08.0477062',
  status: 'Provisional',
  availabilityStatus: 'Supported',
  reminderSet: false,
};

const mockBookings: Booking[] = [
  mockBooking1,
  mockBooking2,
  mockBooking3,
  mockBooking4,
  mockBooking5,
  mockBooking6,
  mockBooking7,
];

/**
 * The weekly summary we'd expect to be generated from @mockWeekAvailability above.
 * Do not modify one of these without updating the other.
 */
const mockWeekAvailability__Summary: DaySummary[] = [
  {
    date: dayjs('2024-06-10 00:00:00'),
    sessions: [
      {
        start: dayjs('2024-06-10 09:00:00'),
        end: dayjs('2024-06-10 12:00:00'),
        maximumCapacity: 72,
        totalBookings: 2,
        bookings: { 'RSV:Adult': 2 },
        capacity: 2,
        slotLength: 5,
      },
      {
        start: dayjs('2024-06-10 13:00:00'),
        end: dayjs('2024-06-10 17:30:00'),
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
    date: dayjs('2024-06-11 00:00:00'),
    sessions: [
      {
        start: dayjs('2024-06-11 09:00:00'),
        end: dayjs('2024-06-11 12:00:00'),
        maximumCapacity: 36,
        totalBookings: 3,
        bookings: { 'RSV:Adult': 2, 'FLU:18_64': 1 },
        capacity: 2,
        slotLength: 10,
      },
      {
        start: dayjs('2024-06-11 09:00:00'),
        end: dayjs('2024-06-11 12:00:00'),
        maximumCapacity: 36,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 2,
        slotLength: 10,
      },
    ],
    maximumCapacity: 72,
    bookedAppointments: 3,
    cancelledAppointments: 1,
    orphanedAppointments: 0,
    remainingCapacity: 69,
  },
  {
    date: dayjs('2024-06-12 00:00:00'),
    sessions: [
      {
        start: dayjs('2024-06-12 08:00:00'),
        end: dayjs('2024-06-12 12:00:00'),
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
    date: dayjs('2024-06-13 00:00:00'),
    sessions: [
      {
        start: dayjs('2024-06-13 10:00:00'),
        end: dayjs('2024-06-13 14:00:00'),
        maximumCapacity: 96,
        totalBookings: 0,
        bookings: { 'RSV:Adult': 0 },
        capacity: 2,
        slotLength: 5,
      },
      {
        start: dayjs('2024-06-13 15:00:00'),
        end: dayjs('2024-06-13 18:00:00'),
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
    date: dayjs('2024-06-14 00:00:00'),
    sessions: [
      {
        start: dayjs('2024-06-14 09:00:00'),
        end: dayjs('2024-06-14 13:00:00'),
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
    date: dayjs('2024-06-15 00:00:00'),
    sessions: [],
    maximumCapacity: 0,
    bookedAppointments: 0,
    cancelledAppointments: 0,
    orphanedAppointments: 0,
    remainingCapacity: 0,
  },
  {
    date: dayjs('2024-06-16 00:00:00'),
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
  bookedAppointments: 5,
  cancelledAppointments: 1,
  orphanedAppointments: 1,
  remainingCapacity: 475,
};

export {
  mockBookings,
  mockWeekAvailability,
  mockWeekAvailability__Summary,
  mondayThe10thOfJune2024,
  sundayThe16thOfJune2024,
  mockWeekSummary,
};
