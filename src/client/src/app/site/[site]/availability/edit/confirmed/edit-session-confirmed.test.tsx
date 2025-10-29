import render from '@testing/render';
import { screen } from '@testing-library/react';
import EditSessionConfirmed from './edit-session-confirmed';
import { mockSingleService, mockSite } from '@testing/data';

describe('Cancellation Confirmed Page', () => {
  it('renders the correct session in the table', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        date="2025-01-15"
        clinicalServices={mockSingleService}
        site={mockSite}
        hasBookings={false}
        chosenAction={''}
        unsupportedBookingsCount={0}
        cancelledWithDetailsCount={0}
        cancelledWithoutDetailsCount={0}
        changeSessionUpliftedJourneyEnabled={false}
      />,
    );

    expect(
      screen.getByRole('row', { name: '09:00 - 12:00 RSV Adult' }),
    ).toBeInTheDocument();
  });

  it('renders a link back to the cancel appointments page', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        clinicalServices={mockSingleService}
        date="2025-01-15"
        site={mockSite}
        hasBookings={false}
        chosenAction={''}
        unsupportedBookingsCount={0}
        cancelledWithDetailsCount={0}
        cancelledWithoutDetailsCount={0}
        changeSessionUpliftedJourneyEnabled={false}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Cancel appointments' }),
    ).toHaveAttribute(
      'href',
      '/site/34e990af-5dc9-43a6-8895-b9123216d699/view-availability/daily-appointments?date=2025-01-15&page=1&tab=2',
    );
  });

  it('renders confirmation and weekly bookings link when no bookings and uplift enabled', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        clinicalServices={mockSingleService}
        date="2025-01-15"
        site={mockSite}
        hasBookings={false}
        chosenAction=""
        unsupportedBookingsCount={0}
        cancelledWithDetailsCount={0}
        cancelledWithoutDetailsCount={0}
        changeSessionUpliftedJourneyEnabled={true}
      />,
    );

    expect(
      screen.getByText('The session has been changed.'),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'View all bookings for this week' }),
    ).toHaveAttribute(
      'href',
      '/site/34e990af-5dc9-43a6-8895-b9123216d699/view-availability/week?date=2025-01-15',
    );
  });

  it('renders cancellation summary with plural card and notification counts', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        clinicalServices={mockSingleService}
        date="2025-01-15"
        site={mockSite}
        hasBookings={true}
        chosenAction="cancel-appointments"
        unsupportedBookingsCount={2}
        cancelledWithDetailsCount={1}
        cancelledWithoutDetailsCount={1}
        changeSessionUpliftedJourneyEnabled={true}
      />,
    );

    expect(
      screen.getByText('Bookings have been cancelled'),
    ).toBeInTheDocument();

    expect(
      screen.getByText('1 person will be sent a text message or email', {
        exact: false,
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/did not provide an email or mobile number/),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: /View the list of people who have not been notified/i,
      }),
    ).toBeInTheDocument();
  });

  it('renders fallback message when unsupported bookings remain', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        clinicalServices={mockSingleService}
        date="2025-01-15"
        site={mockSite}
        hasBookings={true}
        chosenAction="update-session"
        unsupportedBookingsCount={1}
        cancelledWithDetailsCount={0}
        cancelledWithoutDetailsCount={0}
        changeSessionUpliftedJourneyEnabled={true}
      />,
    );

    expect(
      screen.getByText(
        'The session has been updated and a new capacity has been saved.',
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText('1 appointment has not been cancelled.'),
    ).toBeInTheDocument();
  });

  it('renders correct pluralization for cancelledWithDetailsCount', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        clinicalServices={mockSingleService}
        date="2025-01-15"
        site={mockSite}
        hasBookings={true}
        chosenAction="cancel-appointments"
        unsupportedBookingsCount={1}
        cancelledWithDetailsCount={2}
        cancelledWithoutDetailsCount={0}
        changeSessionUpliftedJourneyEnabled={true}
      />,
    );

    expect(
      screen.getByText('2 people will be sent a text message or email', {
        exact: false,
      }),
    ).toBeInTheDocument();
  });

  it('renders fallback message when no bookings and uplift disabled', () => {
    render(
      <EditSessionConfirmed
        updatedSession={{
          from: '09:00',
          until: '12:00',
          services: ['RSV:Adult'],
          capacity: 10,
          slotLength: 5,
        }}
        clinicalServices={mockSingleService}
        date="2025-01-15"
        site={mockSite}
        hasBookings={false}
        chosenAction=""
        unsupportedBookingsCount={0}
        cancelledWithDetailsCount={0}
        cancelledWithoutDetailsCount={0}
        changeSessionUpliftedJourneyEnabled={false}
      />,
    );

    expect(
      screen.getByText(
        /Some booked appointments may be affected by this change/,
      ),
    ).toBeInTheDocument();
  });
});
