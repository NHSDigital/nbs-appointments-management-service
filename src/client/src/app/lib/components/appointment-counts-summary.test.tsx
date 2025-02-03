import render from '@testing/render';
import { screen } from '@testing-library/react';
import { mockDaySummaries } from '@testing/data';
import { AppointmentCountsSummary } from './appointment-counts-summary';
import { DaySummary } from '@types';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';

const mockDaySummary: DaySummary = {
  ...mockDaySummaries[0],
  maximumCapacity: 123,
  bookedAppointments: 5,
  cancelledAppointments: 7,
  orphanedAppointments: 3,
  remainingCapacity: 118,
};

describe('Appointment Counts Summary', () => {
  describe('when given a Day Summary', () => {
    it('renders', () => {
      render(<AppointmentCountsSummary period={mockDaySummary} />);

      expect(screen.getByText('Total appointments: 123')).toBeInTheDocument();
      expect(screen.getByText('Unbooked: 118')).toBeInTheDocument();
    });

    it('sums orphaned and non-orphaned appointments for the booked count', () => {
      render(<AppointmentCountsSummary period={mockDaySummary} />);

      expect(screen.getByText('Booked: 8')).toBeInTheDocument();
    });
  });

  describe('when given a Week Summary', () => {
    it('renders', () => {
      render(<AppointmentCountsSummary period={mockWeekSummary} />);

      expect(screen.getByText('Total appointments: 480')).toBeInTheDocument();
      expect(screen.getByText('Unbooked: 475')).toBeInTheDocument();
    });

    it('sums orphaned and non-orphaned appointments for the booked count', () => {
      render(<AppointmentCountsSummary period={mockWeekSummary} />);

      expect(screen.getByText('Booked: 6')).toBeInTheDocument();
    });
  });
});
