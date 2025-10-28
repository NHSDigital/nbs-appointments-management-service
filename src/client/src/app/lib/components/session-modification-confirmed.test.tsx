import { screen } from '@testing-library/react';
import { SessionModificationConfirmed } from './session-modification-confirmed';
import render from '@testing/render';
import { mockMultipleServices } from '@testing/data';
import { SessionSummary } from '@types';

const mockSessionSummary: SessionSummary = {
  ukStartDatetime: '2025-10-23T10:00:00',
  ukEndDatetime: '2025-10-23T12:00:00',
  maximumCapacity: 24,
  totalSupportedAppointments: 1,
  totalSupportedAppointmentsByService: { 'RSV:Adult': 1 },
  capacity: 2,
  slotLength: 10,
};

describe('CancelSessionConfirmation', () => {
  it('No unsupported bookings, session cancelled', () => {
    render(
      <SessionModificationConfirmed
        clinicalServices={mockMultipleServices}
        siteId="site-123"
        date="2024-06-10"
        modificationAction="cancel-session"
        sessionSummary={mockSessionSummary}
        unsupportedBookingsCount={0}
        bookingsCanceledWithoutDetails={0}
      />,
    );

    expect(
      screen.getByText('This session has been cancelled.'),
    ).toBeInTheDocument();
  });

  it('Session and bookings cancelled, all comms sent out', () => {
    render(
      <SessionModificationConfirmed
        clinicalServices={mockMultipleServices}
        siteId="site-123"
        date="2024-06-10"
        modificationAction="cancel-appointments"
        sessionSummary={mockSessionSummary}
        unsupportedBookingsCount={2}
        bookingsCanceledWithoutDetails={0}
      />,
    );

    expect(
      screen.getByText(/This session has been cancelled/).closest('p'),
    ).toHaveTextContent(
      'This session has been cancelled and 2 bookings have been cancelled.',
    );

    expect(
      screen.getByText(/people have been sent a text/).closest('p'),
    ).toHaveTextContent(
      '2 people have been sent a text message or email confirming their appointment has been cancelled.',
    );
  });

  it('Session and bookings cancelled, bookings missing comms details', async () => {
    render(
      <SessionModificationConfirmed
        clinicalServices={mockMultipleServices}
        siteId="site-123"
        date="2024-06-10"
        modificationAction="cancel-appointments"
        sessionSummary={mockSessionSummary}
        unsupportedBookingsCount={2}
        bookingsCanceledWithoutDetails={1}
      />,
    );

    expect(
      screen.getByText(/This session has been cancelled/).closest('p'),
    ).toHaveTextContent(
      'This session has been cancelled and 2 bookings have been cancelled.',
    );

    expect(
      screen.getByText(/people have been sent a text/).closest('p'),
    ).toHaveTextContent(
      '1 people have been sent a text message or email confirming their appointment has been cancelled.',
    );

    expect(
      screen.getByText(/people did not provide/).closest('p'),
    ).toHaveTextContent('1 people did not provide an email or mobile number');
  });

  it('Has unsupported bookings, no bookings cancelled', async () => {
    render(
      <SessionModificationConfirmed
        clinicalServices={mockMultipleServices}
        siteId="site-123"
        date="2024-06-10"
        modificationAction="keep-appointments"
        sessionSummary={mockSessionSummary}
        unsupportedBookingsCount={2}
        bookingsCanceledWithoutDetails={0}
      />,
    );

    expect(
      screen.getByText(/This session has been cancelled/).closest('p'),
    ).toHaveTextContent(
      'This session has been cancelled. 2 bookings have not been cancelled.',
    );
  });
});
