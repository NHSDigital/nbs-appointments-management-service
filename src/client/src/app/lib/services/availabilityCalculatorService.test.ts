import {
  mockBookings,
  mockWeekAvailability,
  mockWeekSummary,
  mondayThe10thOfJune2024,
  sundayThe16thOfJune2024,
} from '@testing/availability-and-bookings-mock-data';
import {
  divideSessionIntoSlots,
  summariseWeek,
} from './availabilityCalculatorService';
import {
  fetchDailyAvailability,
  fetchBookings,
} from '@services/appointmentsService';
import { Booking, DailyAvailability } from '@types';
import {
  addHoursAndMinutesToUkDatetime,
  dateTimeFormat,
  isAfter,
  isBefore,
  isBeforeOrEqual,
  isEqual,
  parseToUkDatetime,
} from './timeService';

jest.mock('@services/appointmentsService');
const fetchDailyAvailabilityMock = fetchDailyAvailability as jest.Mock<
  Promise<DailyAvailability[]>
>;
const fetchBookingsMock = fetchBookings as jest.Mock<Promise<Booking[]>>;

describe('Availability Calculator Service', () => {
  beforeEach(() => {
    fetchDailyAvailabilityMock.mockReturnValue(
      Promise.resolve(mockWeekAvailability),
    );
    fetchBookingsMock.mockReturnValue(Promise.resolve(mockBookings));
  });

  it('summarises a week of availability with bookings and removes provisional bookings from the count', async () => {
    const weekSummary = await summariseWeek(
      mondayThe10thOfJune2024,
      sundayThe16thOfJune2024,
      'TEST01',
    );

    expect(weekSummary).toEqual(mockWeekSummary);
  });

  it('divideSessionIntoSlots 28th March', async () => {
    const ukDate = parseToUkDatetime('2026-03-28');

    //6 hours long
    const from = addHoursAndMinutesToUkDatetime(ukDate, 8, 0);
    const to = addHoursAndMinutesToUkDatetime(ukDate, 14, 0);

    //4 slots per hour
    const result = divideSessionIntoSlots(0, from, to, {
      services: ['RSV:Adult'],
      slotLength: 15,
      capacity: 10,
      from: '',
      until: '',
    });

    const lastDateTime = parseToUkDatetime(
      '2026-03-28 13:45:00',
      dateTimeFormat,
    );

    expect(result.length).toEqual(24);
    expect(result[0].from.toISOString()).toBe('2026-03-28T08:00:00.000Z');
    expect(result[23].from.toISOString()).toBe('2026-03-28T13:45:00.000Z');

    expect(isEqual(result[23].from, lastDateTime)).toBe(true);
  });

  it('divideSessionIntoSlots 29th March', async () => {
    const ukDate = parseToUkDatetime('2026-03-29');

    //6 hours long
    const from = addHoursAndMinutesToUkDatetime(ukDate, 8, 0);
    const to = addHoursAndMinutesToUkDatetime(ukDate, 14, 0);

    //4 slots per hour
    const result = divideSessionIntoSlots(0, from, to, {
      services: ['RSV:Adult'],
      slotLength: 15,
      capacity: 10,
      from: '',
      until: '',
    });

    const expectedLastDateTime = parseToUkDatetime(
      '2026-03-29 13:45:00',
      dateTimeFormat,
    );

    const lastDateTime = result[23].from;

    expect(result.length).toEqual(24);
    expect(result[0].from.toISOString()).toBe('2026-03-29T07:00:00.000Z');
    expect(lastDateTime.toISOString()).toBe('2026-03-29T12:45:00.000Z');

    //https://github.com/iamkun/dayjs/issues/1189

    //prove that when running these tests in a different TZ, the isSame check fails and therefore can't be used
    //i.e to see, run: TZ="Pacific/Kiritimati" npm test
    expect(lastDateTime.isSame(expectedLastDateTime)).toBe(true);
    //proves we NEED a new isEqual check that uses utc comparison...
    expect(isEqual(lastDateTime, expectedLastDateTime)).toBe(true);

    expect(isBeforeOrEqual(expectedLastDateTime, lastDateTime)).toBe(true);
    expect(isBeforeOrEqual(lastDateTime, expectedLastDateTime)).toBe(true);
    expect(isBefore(lastDateTime, expectedLastDateTime)).toBe(false);
    expect(isAfter(lastDateTime, expectedLastDateTime)).toBe(false);
  });

  it('divideSessionIntoSlots 30th March', async () => {
    const ukDate = parseToUkDatetime('2026-03-30');

    //6 hours long
    const from = addHoursAndMinutesToUkDatetime(ukDate, 8, 0);
    const to = addHoursAndMinutesToUkDatetime(ukDate, 14, 0);

    //4 slots per hour
    const result = divideSessionIntoSlots(0, from, to, {
      services: ['RSV:Adult'],
      slotLength: 15,
      capacity: 10,
      from: '',
      until: '',
    });

    const expectedLastDateTime = parseToUkDatetime(
      '2026-03-30 13:45:00',
      dateTimeFormat,
    );

    const lastDateTime = result[23].from;

    expect(result.length).toEqual(24);
    expect(result[0].from.toISOString()).toBe('2026-03-30T07:00:00.000Z');
    expect(lastDateTime.toISOString()).toBe('2026-03-30T12:45:00.000Z');

    expect(isEqual(lastDateTime, expectedLastDateTime)).toBe(true);
    expect(isBeforeOrEqual(expectedLastDateTime, lastDateTime)).toBe(true);
    expect(isBeforeOrEqual(lastDateTime, expectedLastDateTime)).toBe(true);
    expect(isBefore(lastDateTime, expectedLastDateTime)).toBe(false);
    expect(isAfter(lastDateTime, expectedLastDateTime)).toBe(false);
  });
});
