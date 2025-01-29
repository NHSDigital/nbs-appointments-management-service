import { AvailabilityResponse, Booking } from '@types';
import { fetchBookings, fetchAvailability } from './appointmentsService';
import {
  getDetailedMonthView,
  getWeeksInMonth,
} from './viewAvailabilityService';
import { mockAvailability, mockBookings, mockSite } from '@testing/data';
import dayjs from 'dayjs';

jest.mock('@services/appointmentsService');
const fetchBookedAppointmentsMock = fetchBookings as jest.Mock<
  Promise<Booking[]>
>;
const fetchAvailabilityMock = fetchAvailability as jest.Mock<
  Promise<AvailabilityResponse[]>
>;

describe('View Availability Service', () => {
  beforeEach(() => {
    fetchBookedAppointmentsMock.mockResolvedValue(mockBookings);
    fetchAvailabilityMock.mockResolvedValue(mockAvailability);
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
    const detailedWeeks = await getDetailedMonthView(
      mockSite,
      dayjs('2024-11-10T00:00:00'),
    );

    expect(detailedWeeks.length).toBe(5);

    const lastWeek = detailedWeeks[detailedWeeks.length - 1];
    expect(lastWeek.unbooked).toBe(16);
    expect(lastWeek.booked).toBe(1);
    expect(lastWeek.totalAppointments).toBe(17);

    const rsvAppt = lastWeek.bookedAppointments.find(
      b => b.service === 'RSV (Adult)',
    );
    expect(rsvAppt?.count).toBe(1);
  });
});
