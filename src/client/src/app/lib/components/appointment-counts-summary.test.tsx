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

    it('renders a warning if there is a cancelled appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
            cancelledAppointments: 1,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.getByText(/There is/)).toBeInTheDocument();
      expect(screen.getByText('1')).toBeInTheDocument();
      expect(
        screen.getByText(/cancelled appointment on this day/),
      ).toBeInTheDocument();
    });

    it('renders a warning if there are cancelled appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
            cancelledAppointments: 20,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/cancelled appointments on this day/),
      ).toBeInTheDocument();
    });

    it('does not render a warning if there are no cancelled appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
            cancelledAppointments: 0,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.queryByText(/There are/)).toBeNull();
      expect(
        screen.queryByText(/cancelled appointments on this day/),
      ).toBeNull();
    });

    it('renders a warning if there is an orphaned appointment', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
            cancelledAppointments: 0,
            orphanedAppointments: 1,
          }}
        />,
      );

      expect(screen.getByText(/There is/)).toBeInTheDocument();
      expect(screen.getByText('1')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellation on this day/),
      ).toBeInTheDocument();
    });

    it('renders a warning if there are orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
            cancelledAppointments: 0,
            orphanedAppointments: 20,
          }}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellations on this day/),
      ).toBeInTheDocument();
    });

    it('does not render a warning if there are no orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
            cancelledAppointments: 0,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.queryByText(/There are/)).toBeNull();
      expect(
        screen.queryByText(/cancelled appointments on this day/),
      ).toBeNull();
    });
  });

  describe('when given a Week Summary', () => {
    it('renders', () => {
      render(<AppointmentCountsSummary period={mockWeekSummary} />);

      expect(screen.getByText('Total appointments: 480')).toBeInTheDocument();
      expect(screen.getByText('Unbooked: 476')).toBeInTheDocument();
    });

    it('sums orphaned and non-orphaned appointments for the booked count', () => {
      render(<AppointmentCountsSummary period={mockWeekSummary} />);

      expect(screen.getByText('Booked: 5')).toBeInTheDocument();
    });

    it('renders a warning if there is a cancelled appointment', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            cancelledAppointments: 1,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.getByText(/There is/)).toBeInTheDocument();
      expect(screen.getByText('1')).toBeInTheDocument();
      expect(
        screen.getByText(/cancelled appointment in this week/),
      ).toBeInTheDocument();
    });

    it('renders a warning if there are cancelled appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            cancelledAppointments: 20,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/cancelled appointments in this week/),
      ).toBeInTheDocument();
    });

    it('does not render a warning if there are no cancelled appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            cancelledAppointments: 0,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.queryByText(/There are/)).toBeNull();
      expect(
        screen.queryByText(/cancelled appointments in this week/),
      ).toBeNull();
    });

    it('renders a warning if there is an orphaned appointment', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            cancelledAppointments: 0,
            orphanedAppointments: 1,
          }}
        />,
      );

      expect(screen.getByText(/There is/)).toBeInTheDocument();
      expect(screen.getByText('1')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellation in this week/),
      ).toBeInTheDocument();
    });

    it('renders a warning if there are orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            cancelledAppointments: 0,
            orphanedAppointments: 20,
          }}
        />,
      );

      expect(screen.getByText(/There are/)).toBeInTheDocument();
      expect(screen.getByText('20')).toBeInTheDocument();
      expect(
        screen.getByText(/manual cancellations in this week/),
      ).toBeInTheDocument();
    });

    it('does not render a warning if there are no orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            cancelledAppointments: 0,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(screen.queryByText(/There are/)).toBeNull();
      expect(
        screen.queryByText(/cancelled appointments in this week/),
      ).toBeNull();
    });
  });
});
