import { render, screen } from '@testing-library/react';
import { ViewAvailabilityPage } from './view-availability-page';
import { mockDetailedWeeks } from '@testing/data';

describe('View Availability Page', () => {
  it('renders', async () => {
    render(<ViewAvailabilityPage weeks={mockDetailedWeeks} />);

    expect(
      screen.getByRole('heading', { name: '1 December to 7 December' }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole('table')).toHaveLength(6);
  });

  it('renders the correct information for a week', () => {
    render(<ViewAvailabilityPage weeks={mockDetailedWeeks} />);

    expect(
      screen.getByRole('row', {
        name: 'Total appointments: 30 Booked: 17 Unbooked: 13',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', { name: 'FLU 18-64 5' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', { name: 'COVID 75+ 10' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('row', { name: 'RSV (Adult) 2' }),
    ).toBeInTheDocument();
  });

  it('renders a link for each week', () => {
    render(<ViewAvailabilityPage weeks={mockDetailedWeeks} />);

    expect(screen.getAllByText('View week')).toHaveLength(3);
  });
});
