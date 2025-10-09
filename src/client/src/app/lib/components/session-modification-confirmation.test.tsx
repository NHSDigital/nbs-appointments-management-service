import { screen, waitFor } from '@testing-library/react';
import { SessionModificationConfirmation } from './session-modification-confirmation';
import render from '@testing/render';
import { mockMultipleServices } from '@testing/data';

const mockSessionSummary = {
  id: '1',
  startTime: '09:00',
  endTime: '12:00',
  capacity: 2,
  services: ['RSV:Adult'],
  totalSupportedAppointments: 2,
  totalUnsupportedAppointments: 0,
  totalAppointments: 2,
  totalSupportedAppointmentsByService: {
    'RSV:Adult': 2,
  },
};

describe('EditSessionConfirmation', () => {
  it('No unsupported bookings, renders question to change session', () => {
    render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={0}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="edit"
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Change session' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Continue' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('Are you sure you want to change this session?'),
    ).toBeInTheDocument();
  });

  it('Has unsupported bookings, renders Yes/No question to cancel the appointments', () => {
    render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="edit"
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Continue' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Change session' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByLabelText(/Yes, cancel the appointments/),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText(/No, do not cancel the appointments/),
    ).toBeInTheDocument();
  });

  it('Has unsupported bookings, user choose "Yes" to cancel the appointments', async () => {
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="edit"
      />,
    );

    await user.click(screen.getByLabelText(/Yes, cancel the appointments/));
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Cancel appointments' }),
      ).toBeInTheDocument();
    });
  });

  it('Has unsupported bookings, user choose "No" to cancel the appointments', async () => {
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="edit"
      />,
    );

    await user.click(
      screen.getByLabelText(/No, do not cancel the appointments/),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Change session' }),
      ).toBeInTheDocument();
    });
  });
});

describe('CancelSessionConfirmation', () => {
  it('No unsupported bookings, renders question to cancel session', () => {
    render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={0}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="cancel"
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Cancel session' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Continue' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('Are you sure you want to cancel the session?'),
    ).toBeInTheDocument();
  });

  it('Has unsupported bookings, renders Yes/No question to cancel the session', () => {
    render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="cancel"
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Continue' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Change session' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByLabelText(
        /Yes, cancel the appointments and cancel the session/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText(
        /No, do not cancel the appointments but cancel the session/,
      ),
    ).toBeInTheDocument();
  });

  it('Has unsupported bookings, user choose "Yes" to cancel the appointments', async () => {
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="cancel"
      />,
    );

    await user.click(screen.getByLabelText(/Yes, cancel the appointments/));
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Cancel appointments' }),
      ).toBeInTheDocument();
    });
  });

  it('Has unsupported bookings, user choose "No" to cancel the appointments', async () => {
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="cancel"
      />,
    );

    await user.click(
      screen.getByLabelText(/No, do not cancel the appointments/),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Cancel session' }),
      ).toBeInTheDocument();
    });
  });
});
