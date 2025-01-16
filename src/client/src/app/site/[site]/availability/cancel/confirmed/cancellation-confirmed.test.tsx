import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import render from '@testing/render';
import { screen } from '@testing-library/react';
import CancellationConfirmed from './cancellation-confirmed';

describe('Cancellation Confirmed Page', () => {
  it('renders the correct session in the table', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <CancellationConfirmed
        session={session}
        date="2025-01-15"
        site="TEST01"
      />,
    );

    expect(
      screen.getByRole('row', { name: '09:00 - 12:00 RSV (Adult)' }),
    ).toBeInTheDocument();
  });

  it('renders a link back to the cancel appointments page', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <CancellationConfirmed
        session={session}
        date="2025-01-15"
        site="TEST01"
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toHaveAttribute(
      'href',
      '/site/TEST01/view-availability/daily-appointments?date=2025-01-15&page=1&tab=2',
    );
  });
});
