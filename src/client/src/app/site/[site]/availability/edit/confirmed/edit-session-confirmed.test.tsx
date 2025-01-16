import render from '@testing/render';
import { screen } from '@testing-library/react';
import EditSessionConfirmed from './edit-session-confirmed';
import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import { mockSite } from '@testing/data';

describe('Cancellation Confirmed Page', () => {
  it('renders the correct session in the table', () => {
    render(
      <EditSessionConfirmed
        sessionSummary={mockWeekAvailability__Summary[0].sessions[0]}
        date="2025-01-15"
        site={mockSite}
      />,
    );

    expect(
      screen.getByRole('row', { name: '09:00 - 12:00 RSV (Adult)' }),
    ).toBeInTheDocument();
  });

  it('renders a link back to the cancel appointments page', () => {
    render(
      <EditSessionConfirmed
        sessionSummary={mockWeekAvailability__Summary[0].sessions[0]}
        date="2025-01-15"
        site={mockSite}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toHaveAttribute(
      'href',
      '/site/1001/view-availability/daily-appointments?date=2025-01-15&page=1&tab=2',
    );
  });
});
