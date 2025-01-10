import {
  mockBookings,
  mockWeekAvailability,
  mockWeekAvailabilityEnd,
  mockWeekAvailabilityStart,
} from '@testing/data';
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

  it('can build a details week for availability', async () => {
    const weekSummary = await summariseWeek(
      mockWeekAvailabilityStart,
      mockWeekAvailabilityEnd,
      'TEST01',
    );

    expect(weekSummary.length).toBe(7);

    const firstDay = weekSummary[0];

    expect(firstDay.remainingCapacity).toBe(191);
    expect(firstDay.bookedAppointments).toBe(1);
    expect(firstDay.maximumCapacity).toBe(192);
    expect(firstDay.sessions).toHaveLength(2);

    expect(Object.keys(firstDay.sessions[0].bookings)).toHaveLength(1);
    expect(Object.keys(firstDay.sessions[1].bookings)).toHaveLength(0);
  });
});
