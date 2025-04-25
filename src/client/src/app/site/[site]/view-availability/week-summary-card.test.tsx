import { render, screen } from '@testing-library/react';
import { WeekSummaryCard } from './week-summary-card';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';
import { clinicalServices } from '@types';

jest.mock('@types', () => ({
  ...jest.requireActual('@types'),
  clinicalServices: [
    { label: 'RSV (Adult)', value: 'RSV:Adult' },
    { label: 'FLU 18-64', value: 'FLU:18_64' },
    { label: 'COVID 75+', value: 'COVID:75+' },
  ],
}));

describe('Week Summary Card', () => {
  it('renders', () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={mockWeekSummary}
        clinicalServices={clinicalServices}
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
        clinicalServices={clinicalServices}
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
        name: 'RSV (Adult) 3',
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
        clinicalServices={clinicalServices}
      />,
    );

    expect(screen.getByText('Total appointments: 480')).toBeInTheDocument();
    expect(screen.getByText('Booked: 5')).toBeInTheDocument();
    expect(screen.getByText('Unbooked: 476')).toBeInTheDocument();
  });

  it('renders a warning if there are cancelled appointments', () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={{ ...mockWeekSummary, cancelledAppointments: 20 }}
        clinicalServices={clinicalServices}
      />,
    );

    expect(screen.getByText(/There are/)).toBeInTheDocument();
    expect(screen.getByText('20')).toBeInTheDocument();
    expect(
      screen.getByText(/cancelled appointments in this week./),
    ).toBeInTheDocument();
  });

  it('renders a warning if there are orphaned appointments', () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={{ ...mockWeekSummary, orphanedAppointments: 31 }}
        clinicalServices={clinicalServices}
      />,
    );

    expect(screen.getByText(/There are/)).toBeInTheDocument();
    expect(screen.getByText('31')).toBeInTheDocument();
    expect(
      screen.getByText(/manual cancellations in this week./),
    ).toBeInTheDocument();
  });

  it('renders a link to week view', async () => {
    render(
      <WeekSummaryCard
        ukWeekSummary={mockWeekSummary}
        clinicalServices={clinicalServices}
      />,
    );

    expect(screen.getByRole('link', { name: 'View week' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'View week' })).toHaveAttribute(
      'href',
      `view-availability/week?date=2024-06-10`,
    );
  });
});
