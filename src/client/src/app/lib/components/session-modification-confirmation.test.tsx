import { screen, waitFor } from '@testing-library/react';
import { SessionModificationConfirmation } from './session-modification-confirmation';
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
  totalSupportedAppointmentsByService: { 'RSV:Adult': 1 },
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

  it('sends correct payload for change session and cancel appointments action', async () => {
    (timeService.toTimeFormat as jest.Mock)
      .mockReturnValueOnce('09:00') // for sessionSummary.ukStartDatetime
      .mockReturnValueOnce('12:00'); // for sessionSummary.ukEndDatetime
    const mode = 'edit';
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode={mode}
      />,
    );

    await user.click(
      screen.getByLabelText(
        'Yes, cancel the appointments and change this session',
      ),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await user.click(
      screen.getByRole('button', {
        name: 'Cancel appointments',
      }),
    );

    // Validate payload
    const cancelUnsupportedBookings = true;
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '09:00',
        until: '12:00',
        services: ['RSV:Adult'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: null,
      cancelUnsupportedBookings: cancelUnsupportedBookings,
    });

    // Validate navigation
    expect(mockPush).toHaveBeenCalled();

    const calledArg = (mockPush.mock.calls[0] as unknown[])[0] as string;
    expect(calledArg).toContain(
      `/site/site-123/availability/${mode}/confirmed`,
    );

    const query = calledArg.split('?')[1] ?? '';
    const params = new URLSearchParams(query);

    const updatedSession = params.get('updatedSession');
    expect(updatedSession).toBeTruthy();
    expect(updatedSession).not.toBe('undefined');
    expect(query).toMatch(/updatedSession=[^&]+/);

    const decoded = updatedSession ? decodeURIComponent(updatedSession) : '';
    expect(decoded.length).toBeGreaterThan(0);

    // other exact params
    expect(params.get('date')).toBe('2024-06-10');
    expect(params.get('chosenAction')).toBe('cancel-appointments');
    expect(params.get('unsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('true');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');
  });

  it('sends correct payload for change session action', async () => {
    (timeService.toTimeFormat as jest.Mock)
      .mockReturnValueOnce('09:00') // for sessionSummary.ukStartDatetime
      .mockReturnValueOnce('12:00'); // for sessionSummary.ukEndDatetime
    const mode = 'edit';
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode={mode}
      />,
    );

    await user.click(
      screen.getByLabelText(
        'No, do not cancel the appointments but change this session',
      ),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await user.click(
      screen.getByRole('button', {
        name: 'Change session',
      }),
    );

    // Validate payload
    const cancelUnsupportedBookings = false;
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '09:00',
        until: '12:00',
        services: ['RSV:Adult'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: null,
      cancelUnsupportedBookings: cancelUnsupportedBookings,
    });

    // Validate navigation
    expect(mockPush).toHaveBeenCalled();

    const calledArg = (mockPush.mock.calls[0] as unknown[])[0] as string;
    expect(calledArg).toContain(
      `/site/site-123/availability/${mode}/confirmed`,
    );

    const query = calledArg.split('?')[1] ?? '';
    const params = new URLSearchParams(query);

    const updatedSession = params.get('updatedSession');
    expect(updatedSession).toBeTruthy();
    expect(updatedSession).not.toBe('undefined');
    expect(query).toMatch(/updatedSession=[^&]+/);

    const decoded = updatedSession ? decodeURIComponent(updatedSession) : '';
    expect(decoded.length).toBeGreaterThan(0);

    // other exact params
    expect(params.get('date')).toBe('2024-06-10');
    expect(params.get('chosenAction')).toBe('change-session');
    expect(params.get('unsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('false');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');
  });

  it('sends correct payload for cancel session action', async () => {
    (timeService.toTimeFormat as jest.Mock)
      .mockReturnValueOnce('09:00') // for sessionSummary.ukStartDatetime
      .mockReturnValueOnce('12:00'); // for sessionSummary.ukEndDatetime
    const mode = 'cancel';
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode={mode}
      />,
    );

    await user.click(
      screen.getByLabelText(
        'No, do not cancel the appointments but cancel the session',
      ),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await user.click(
      screen.getByRole('button', {
        name: 'Cancel session',
      }),
    );

    // Validate payload
    const cancelUnsupportedBookings = false;
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '09:00',
        until: '12:00',
        services: ['RSV:Adult'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: null,
      cancelUnsupportedBookings: cancelUnsupportedBookings,
    });

    // Validate navigation
    expect(mockPush).toHaveBeenCalled();

    const calledArg = (mockPush.mock.calls[0] as unknown[])[0] as string;
    expect(calledArg).toContain(
      `/site/site-123/availability/${mode}/confirmed`,
    );

    const query = calledArg.split('?')[1] ?? '';
    const params = new URLSearchParams(query);

    const updatedSession = params.get('updatedSession');
    expect(updatedSession).toBeTruthy();
    expect(updatedSession).not.toBe('undefined');
    expect(query).toMatch(/updatedSession=[^&]+/);

    const decoded = updatedSession ? decodeURIComponent(updatedSession) : '';
    expect(decoded.length).toBeGreaterThan(0);

    // other exact params
    expect(params.get('date')).toBe('2024-06-10');
    expect(params.get('chosenAction')).toBe('keep-appointments');
    expect(params.get('unsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('false');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');
  });

  it('sends correct payload for cancel session and cancel appointments action', async () => {
    (timeService.toTimeFormat as jest.Mock)
      .mockReturnValueOnce('09:00') // for sessionSummary.ukStartDatetime
      .mockReturnValueOnce('12:00'); // for sessionSummary.ukEndDatetime
    const mode = 'cancel';
    const { user } = render(
      <SessionModificationConfirmation
        unsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode={mode}
      />,
    );

    await user.click(
      screen.getByLabelText(
        'Yes, cancel the appointments and cancel the session',
      ),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await user.click(
      screen.getByRole('button', {
        name: 'Cancel appointments',
      }),
    );

    // Validate payload
    const cancelUnsupportedBookings = true;
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '09:00',
        until: '12:00',
        services: ['RSV:Adult'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: null,
      cancelUnsupportedBookings: cancelUnsupportedBookings,
    });

    // Validate navigation
    //expect(mockPush).toHaveBeenCalledWith(
    //  expect.stringContaining(
    //    `/site/site-123/availability/${mode}/confirmed?updatedSession=undefined&date=2024-06-10&chosenAction=cancel-appointments&unsupportedBookingsCount=2&cancelAppointments=${cancelUnsupportedBookings}`,
    //  ),
    //);

    expect(mockPush).toHaveBeenCalled();

    const calledArg = (mockPush.mock.calls[0] as unknown[])[0] as string;
    expect(calledArg).toContain(
      `/site/site-123/availability/${mode}/confirmed`,
    );

    const query = calledArg.split('?')[1] ?? '';
    const params = new URLSearchParams(query);

    const updatedSession = params.get('updatedSession');
    expect(updatedSession).toBeTruthy();
    expect(updatedSession).not.toBe('undefined');
    expect(query).toMatch(/updatedSession=[^&]+/);

    const decoded = updatedSession ? decodeURIComponent(updatedSession) : '';
    expect(decoded.length).toBeGreaterThan(0);

    // other exact params
    expect(params.get('date')).toBe('2024-06-10');
    expect(params.get('chosenAction')).toBe('cancel-appointments');
    expect(params.get('unsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('true');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');
  });
});
