import { render, screen } from '@testing-library/react';
import { WeekSummaryCard } from './week-summary-card';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';
import { mockMultipleServices } from '@testing/data';
import { isThisWeek } from '@services/timeService';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    isThisWeek: jest.fn(),
  };
});
const mockIsThisWeek = isThisWeek as jest.Mock<boolean>;

describe('Week Summary Card', () => {
  it('renders', () => {
    mockIsThisWeek.mockReturnValue(false);
    render(
      <WeekSummaryCard
        ukWeekSummary={mockWeekSummary}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '10 June to 16 June' }),
    ).toBeInTheDocument();
  });

  it('renders a table with appointments by service', async () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={mockWeekSummary}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: 'Services Booked appointments',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: 'RSV Adult 3',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: 'FLU 18-64 1',
      }),
    ).toBeInTheDocument();
  });

  it('renders an appointment counts summary', async () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={mockWeekSummary}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.getByText('Total appointments: 481')).toBeInTheDocument();
    expect(screen.getByText('Booked: 5')).toBeInTheDocument();
    expect(screen.getByText('Unbooked: 476')).toBeInTheDocument();
  });

  it('renders a warning if there are orphaned appointments', () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={{ ...mockWeekSummary, orphanedAppointments: 31 }}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(
      screen.getByText(
        /31 bookings were kept when availability was changed or cancelled./,
      ),
    ).toBeInTheDocument();
  });

  it('renders a warning if there is a single orphaned appointment', () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={{ ...mockWeekSummary, orphanedAppointments: 1 }}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(
      screen.getByText(
        /1 booking was kept when availability was changed or cancelled./,
      ),
    ).toBeInTheDocument();
  });

  it('renders a link to week view', async () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={mockWeekSummary}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(screen.getByRole('link', { name: 'View week' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'View week' })).toHaveAttribute(
      'href',
      `view-availability/week?date=2024-06-10`,
    );
  });

  it('Adds this week label', async () => {
    mockIsThisWeek.mockReturnValue(true);
    render(
      <WeekSummaryCard
        ukWeekSummary={mockWeekSummary}
        clinicalServices={mockMultipleServices}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '10 June to 16 June (this week)' }),
    ).toBeInTheDocument();
  });
});
