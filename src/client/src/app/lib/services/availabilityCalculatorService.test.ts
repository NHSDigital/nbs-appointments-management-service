import {
  mockBookings,
  mockWeekAvailability,
  mockWeekSummary,
  mondayThe10thOfJune2024,
  sundayThe16thOfJune2024,
} from '@testing/availability-and-bookings-mock-data';
import { summariseWeek } from './availabilityCalculatorService';
import {
  fetchDailyAvailability,
  fetchBookings,
} from '@services/appointmentsService';
import { Booking, DailyAvailability } from '@types';

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

  it('summarises a week of availability with bookings', async () => {
    const weekSummary = await summariseWeek(
      mondayThe10thOfJune2024,
      sundayThe16thOfJune2024,
      'TEST01',
    );

    expect(weekSummary).toEqual(mockWeekSummary);
  });
});
