import { Booking } from '@types';
import { fetchBookings } from './appointmentsService';
import {
  getDetailedMonthView,
  getWeeksInMonth,
} from './viewAvailabilityService';
import { mockAvailability, mockBookings } from '@testing/data';

jest.mock('@services/appointmentsService');
const fetchBookedAppointmentsMock = fetchBookings as jest.Mock<
  Promise<Booking[]>
>;

describe('View Availability Service', () => {
  beforeEach(() => {
    fetchBookedAppointmentsMock.mockResolvedValue(mockBookings);
  });

  it.each([
    [2024, 9, 8, 10, 5],
    [2024, 10, 9, 11, 5],
    [2024, 11, 10, 0, 6],
    [2025, 0, 11, 1, 5],
  ])(
    'gets all full weeks in month plus week start and end days from other months',
    (
      year: number,
      month: number,
      exectedStartMonth,
      expectedEndMonth,
      expectedWeekCount,
    ) => {
      const weeks = getWeeksInMonth(year, month);
      const firstWeek = weeks[0];
      const lastWeek = weeks[weeks.length - 1];

      expect(weeks.length).toBe(expectedWeekCount);
      expect(firstWeek.startDate.month()).toBe(exectedStartMonth);
      expect(lastWeek.endDate.month()).toBe(expectedEndMonth);
    },
  );

  it('can build a detailed month view for availability', async () => {
    const weeks = getWeeksInMonth(2024, 10);
    const detailedWeeks = await getDetailedMonthView(
      mockAvailability,
      weeks,
      'TEST01',
    );

    expect(detailedWeeks.length).toBe(5);
    expect(detailedWeeks[1].unbooked).toBe(16);
    expect(detailedWeeks[1].booked).toBe(1);
    expect(detailedWeeks[1].totalAppointments).toBe(17);

    const rsvAppt = detailedWeeks[1].bookedAppointments.find(
      b => b.service === 'RSV (Adult)',
    );
    expect(rsvAppt?.count).toBe(1);
  });
});
