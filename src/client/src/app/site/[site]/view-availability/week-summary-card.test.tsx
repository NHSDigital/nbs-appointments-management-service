import { render, screen } from '@testing-library/react';
import { WeekSummaryCard } from './week-summary-card';
import { mockWeekSummary } from '@testing/availability-and-bookings-mock-data';

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
    render(<WeekSummaryCard weekSummary={mockWeekSummary} />);

    expect(
      screen.getByRole('heading', { name: '10 June to 16 June' }),
    ).toBeInTheDocument();
  });

  it('renders a table with appointments by service', async () => {
    render(<WeekSummaryCard weekSummary={mockWeekSummary} />);

    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: 'Services Booked appointments',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: 'RSV (Adult) 4',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('row', {
        name: 'FLU 18-64 1',
      }),
    ).toBeInTheDocument();
  });

  it('renders an appointment counts summary', async () => {
    render(<WeekSummaryCard weekSummary={mockWeekSummary} />);

    expect(screen.getByText('Total appointments: 480')).toBeInTheDocument();
    expect(screen.getByText('Booked: 6')).toBeInTheDocument();
    expect(screen.getByText('Unbooked: 475')).toBeInTheDocument();
  });

  it('renders a warning if there are cancelled appointments', () => {
    render(
      <WeekSummaryCard
        weekSummary={{ ...mockWeekSummary, cancelledAppointments: 20 }}
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
        weekSummary={{ ...mockWeekSummary, orphanedAppointments: 31 }}
      />,
    );

    expect(screen.getByText(/There are/)).toBeInTheDocument();
    expect(screen.getByText('31')).toBeInTheDocument();
    expect(
      screen.getByText(/manual cancellations in this week./),
    ).toBeInTheDocument();
  });

  it('renders a link to week view', async () => {
    render(<WeekSummaryCard weekSummary={mockWeekSummary} />);

    expect(screen.getByRole('link', { name: 'View week' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'View week' })).toHaveAttribute(
      'href',
      `view-availability/week?date=2024-06-10`,
    );
  });
});
