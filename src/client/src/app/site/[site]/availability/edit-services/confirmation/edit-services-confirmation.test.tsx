import { screen, waitFor } from '@testing-library/react';
import { EditServicesConfirmationPage } from './edit-services-confirmation';
import render from '@testing/render';
import { mockMultipleServices } from '@testing/data';
import { useRouter } from 'next/navigation';
import * as appointmentsService from '@services/appointmentsService';
import asServerActionResult from '@testing/asServerActionResult';
import * as timeService from '@services/timeService';
import { SessionModificationResponse } from '@types';

const mockSessionSummary = {
  ukStartDatetime: '2025-10-23T10:00:00',
  ukEndDatetime: '2025-10-23T12:00:00',
  maximumCapacity: 24,
  totalSupportedAppointments: 1,
  totalSupportedAppointmentsByService: { 'RSV:Adult': 2, 'Flu:65+': 1 },
  capacity: 2,
  slotLength: 10,
};

const mockRemovedService = {
  from: '2025-10-23T10:00:00',
  until: '2025-10-23T12:00:00',
  services: ['Flu:65+'],
  capacity: 2,
  slotLength: 10,
};

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

jest.mock('@services/appointmentsService');
const mockModifySession = jest.spyOn(appointmentsService, 'modifySession');

jest.mock('@services/timeService', () => ({
  ...jest.requireActual('@services/timeService'),
  toTimeFormat: jest.fn(),
}));

describe('EditSessionConfirmation', () => {
  it('No unsupported bookings, renders question to change session', () => {
    render(
      <EditServicesConfirmationPage
        newlyOrphanedBookingsCount={0}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        removedServicesSession={btoa(JSON.stringify(mockRemovedService))}
        site="site-123"
        date="2024-06-10"
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Remove service' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Continue' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('Are you sure you want to remove this service?'),
    ).toBeInTheDocument();
  });
});

describe('CancelSessionConfirmation', () => {
  it('No unsupported bookings, renders question to remove the service', () => {
    render(
      <EditServicesConfirmationPage
        newlyOrphanedBookingsCount={0}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        removedServicesSession={btoa(JSON.stringify(mockRemovedService))}
        site="site-123"
        date="2024-06-10"
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Remove service' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Continue' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('Are you sure you want to remove this service?'),
    ).toBeInTheDocument();
  });

  it('Has unsupported bookings, renders Yes/No question to remove the service', () => {
    render(
      <EditServicesConfirmationPage
        newlyOrphanedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        removedServicesSession={btoa(JSON.stringify(mockRemovedService))}
        site="site-123"
        date="2024-06-10"
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
        /Yes, cancel the appointments and remove the service/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText(
        /No, do not cancel the appointments but remove the service/,
      ),
    ).toBeInTheDocument();
  });

  it('Has unsupported bookings, user choose "Yes" to cancel the appointments', async () => {
    const { user } = render(
      <EditServicesConfirmationPage
        newlyOrphanedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        removedServicesSession={btoa(JSON.stringify(mockRemovedService))}
        site="site-123"
        date="2024-06-10"
      />,
    );

    await user.click(screen.getByLabelText(/Yes, cancel the appointment/));
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Cancel appointments' }),
      ).toBeInTheDocument();
    });
  });

  it('Has unsupported bookings, user choose "No" to cancel the appointments', async () => {
    const { user } = render(
      <EditServicesConfirmationPage
        newlyOrphanedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        removedServicesSession={btoa(JSON.stringify(mockRemovedService))}
        site="site-123"
        date="2024-06-10"
      />,
    );

    await user.click(
      screen.getByLabelText(/No, do not cancel the appointment/),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Remove service' }),
      ).toBeInTheDocument();
    });
  });
});

describe('submitForm', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({ push: mockPush });
    const sessionModificationResult: SessionModificationResponse = {
      updateSuccessful: true,
      message: true,
      bookingsCanceled: 2,
      bookingsCanceledWithoutDetails: 1,
    };

    mockModifySession.mockResolvedValue(
      asServerActionResult<SessionModificationResponse>(
        sessionModificationResult,
      ),
    );
    jest.clearAllMocks();
  });

  it('sends correct payload for edit services action', async () => {
    (timeService.toTimeFormat as jest.Mock)
      .mockReturnValueOnce('09:00') // for sessionSummary.ukStartDatetime
      .mockReturnValueOnce('12:00'); // for sessionSummary.ukEndDatetime
    const mode = 'edit-services';
    const { user } = render(
      <EditServicesConfirmationPage
        newlyOrphanedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        removedServicesSession={btoa(JSON.stringify(mockRemovedService))}
        site="site-123"
        date="2024-06-10"
      />,
    );

    await user.click(
      screen.getByLabelText(
        'No, do not cancel the appointments but remove the service',
      ),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await user.click(
      screen.getByRole('button', {
        name: 'Remove service',
      }),
    );

    // Validate payload
    const cancelNewlyOrphanedBookings = false;
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '09:00',
        until: '12:00',
        services: ['RSV:Adult', 'Flu:65+'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: {
        from: '2025-10-23T10:00:00',
        until: '2025-10-23T12:00:00',
        services: ['RSV:Adult'],
        slotLength: 10,
        capacity: 2,
      },
      cancelNewlyOrphanedBookings: cancelNewlyOrphanedBookings,
    });

    // Validate navigation
    expect(mockPush).toHaveBeenCalled();

    const calledArg = (mockPush.mock.calls[0] as unknown[])[0] as string;
    expect(calledArg).toContain(
      `/site/site-123/availability/${mode}/confirmed`,
    );

    const query = calledArg.split('?')[1] ?? '';
    const params = new URLSearchParams(query);

    const updatedSession = params.get('session');
    expect(updatedSession).toBeTruthy();
    expect(updatedSession).not.toBe('undefined');
    expect(query).toMatch(/session=[^&]+/);

    const decoded = updatedSession ? decodeURIComponent(updatedSession) : '';
    expect(decoded.length).toBeGreaterThan(0);

    // other exact params
    expect(params.get('date')).toBe('2024-06-10');
    expect(params.get('chosenAction')).toBe('remove-services');
    expect(params.get('newlyOrphanedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('false');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');
  });

  it('sends correct payload for edit services and cancel appointments action', async () => {
    (timeService.toTimeFormat as jest.Mock)
      .mockReturnValueOnce('09:00') // for sessionSummary.ukStartDatetime
      .mockReturnValueOnce('12:00'); // for sessionSummary.ukEndDatetime
    const mode = 'edit-services';
    const { user } = render(
      <EditServicesConfirmationPage
        newlyOrphanedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        removedServicesSession={btoa(JSON.stringify(mockRemovedService))}
        site="site-123"
        date="2024-06-10"
      />,
    );

    await user.click(
      screen.getByLabelText(
        'Yes, cancel the appointments and remove the service',
      ),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await user.click(
      screen.getByRole('button', {
        name: 'Cancel appointments',
      }),
    );

    // Validate payload
    const cancelNewlyOrphanedBookings = true;
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '09:00',
        until: '12:00',
        services: ['RSV:Adult', 'Flu:65+'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: {
        from: '2025-10-23T10:00:00',
        until: '2025-10-23T12:00:00',
        services: ['RSV:Adult'],
        slotLength: 10,
        capacity: 2,
      },
      cancelNewlyOrphanedBookings: cancelNewlyOrphanedBookings,
    });

    expect(mockPush).toHaveBeenCalled();

    const calledArg = (mockPush.mock.calls[0] as unknown[])[0] as string;
    expect(calledArg).toContain(
      `/site/site-123/availability/${mode}/confirmed`,
    );

    const query = calledArg.split('?')[1] ?? '';
    const params = new URLSearchParams(query);

    const updatedSession = params.get('session');
    expect(updatedSession).toBeTruthy();
    expect(updatedSession).not.toBe('undefined');
    expect(query).toMatch(/session=[^&]+/);

    const decoded = updatedSession ? decodeURIComponent(updatedSession) : '';
    expect(decoded.length).toBeGreaterThan(0);

    // other exact params
    expect(params.get('date')).toBe('2024-06-10');
    expect(params.get('chosenAction')).toBe('cancel-appointments');
    expect(params.get('newlyOrphanedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('true');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');
  });
});
