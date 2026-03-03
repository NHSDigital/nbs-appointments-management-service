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
  orphanedAppointments: 3,
  remainingCapacity: 118,
};

describe('Appointment Counts Summary', () => {
  describe('when given a Day Summary', () => {
    it('renders', () => {
      render(<AppointmentCountsSummary period={mockDaySummary} />);

      expect(screen.getByText('Total appointments: 126')).toBeInTheDocument();
      expect(screen.getByText('Unbooked: 118')).toBeInTheDocument();
    });

    it('sums orphaned and non-orphaned appointments for the booked count', () => {
      render(<AppointmentCountsSummary period={mockDaySummary} />);

      expect(screen.getByText('Booked: 8')).toBeInTheDocument();
    });

    it('does not render a warning if there are no cancelled appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
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
            orphanedAppointments: 1,
          }}
        />,
      );

      expect(
        screen.getByText(
          /1 booking was kept when availability was changed or cancelled./,
        ),
      ).toBeInTheDocument();
    });

    it('renders a warning if there are orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
            orphanedAppointments: 20,
          }}
        />,
      );

      expect(
        screen.getByText(
          /20 bookings were kept when availability was changed or cancelled./,
        ),
      ).toBeInTheDocument();
    });

    it('does not render a warning if there are no orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockDaySummary,
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

      expect(screen.getByText('Total appointments: 481')).toBeInTheDocument();
      expect(screen.getByText('Unbooked: 476')).toBeInTheDocument();
    });

    it('sums orphaned bookings and total possible slots for the total appointments count', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            orphanedAppointments: 22,
          }}
        />,
      );

      expect(screen.getByText('Total appointments: 502')).toBeInTheDocument();
    });

    it('sums orphaned and non-orphaned appointments for the booked count', () => {
      render(<AppointmentCountsSummary period={mockWeekSummary} />);

      expect(screen.getByText('Booked: 5')).toBeInTheDocument();
    });

    it('renders a warning if there is an orphaned appointment', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            orphanedAppointments: 1,
          }}
        />,
      );

      expect(
        screen.getByText(
          /1 booking was kept when availability was changed or cancelled./,
        ),
      ).toBeInTheDocument();
    });

    it('renders a warning if there are orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            orphanedAppointments: 20,
          }}
        />,
      );

      expect(
        screen.getByText(
          /20 bookings were kept when availability was changed or cancelled./,
        ),
      ).toBeInTheDocument();
    });

    it('does not render a warning if there are no orphaned appointments', () => {
      render(
        <AppointmentCountsSummary
          period={{
            ...mockWeekSummary,
            orphanedAppointments: 0,
          }}
        />,
      );

      expect(
        screen.queryByText(
          /bookings were kept when availability was changed or cancelled./,
        ),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByText(
          /booking was kept when availability was changed or cancelled./,
        ),
      ).not.toBeInTheDocument();
    });
  });
});
