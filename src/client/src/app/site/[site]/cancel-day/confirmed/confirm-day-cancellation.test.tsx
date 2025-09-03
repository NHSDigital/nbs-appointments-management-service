import { mockCancelDayResponse } from '@testing/data';
import CancellationConfirmed from './confirm-day-cancellation';
import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CancelDayResponse } from '@types';

describe('Cancellation Confirmed  Page', () => {
  it('renders', () => {
    render(
      <CancellationConfirmed
        dayCancellationSummary={mockCancelDayResponse}
        site="TEST01"
        date="2025-09-01"
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'View bookings without contact details',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'View all bookings for this week' }),
    ).toBeInTheDocument();
  });

  it('calculates the correct values', () => {
    render(
      <CancellationConfirmed
        dayCancellationSummary={mockCancelDayResponse}
        site="TEST01"
        date="2025-09-01"
      />,
    );

    expect(
      screen.getByText(/10 appointments have been cancelled/i),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        /8 people will be sent a text message or email confirming their appointment has been cancelled./i,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        /2 people did not provide contact details so they will not receive a notification./i,
      ),
    ).toBeInTheDocument();
  });

  it('hides the link when there are no bookings without contact details', () => {
    const dayCancellationSummary: CancelDayResponse = {
      bookingsWithoutContactDetails: 0,
      cancelledBookingCount: 5,
    };

    render(
      <CancellationConfirmed
        dayCancellationSummary={dayCancellationSummary}
        site="TEST01"
        date="2025-09-01"
      />,
    );

    expect(
      screen.queryByText(/^View bookings without contact details/),
    ).not.toBeInTheDocument();
  });
});
