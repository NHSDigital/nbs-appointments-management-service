import { render, screen } from '@testing-library/react';
import { ViewWeekAvailabilityPage } from './view-week-availability-page';
import { mockDailyAvailability } from '@testing/data';

describe('View Week Availability Page', () => {
  it('renders', async () => {
    render(<ViewWeekAvailabilityPage availability={mockDailyAvailability} />);

    expect(
      screen.getByRole('heading', { name: 'Monday 2 December' }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole('table')).toHaveLength(6);
  });

  // TODO: Add tests for correct information (booked, unbooked etc.)
});
