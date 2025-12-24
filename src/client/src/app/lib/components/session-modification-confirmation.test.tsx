import { screen, waitFor } from '@testing-library/react';
import { SessionModificationConfirmation } from './session-modification-confirmation';
import render from '@testing/render';
import { localStorageMock, mockMultipleServices } from '@testing/data';
import { useRouter } from 'next/navigation';
import * as appointmentsService from '@services/appointmentsService';
import asServerActionResult from '@testing/asServerActionResult';
import { SessionModificationResponse, SessionSummary } from '@types';

const mockSessionSummary: SessionSummary = {
  ukStartDatetime: '2025-10-23T10:00:00',
  ukEndDatetime: '2025-10-23T12:00:00',
  maximumCapacity: 24,
  totalSupportedAppointments: 1,
  totalSupportedAppointmentsByService: { 'RSV:Adult': 3, 'FLU:18_64': 5 },
  capacity: 2,
  slotLength: 10,
};

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

jest.mock('@services/appointmentsService');
const mockModifySession = jest.spyOn(appointmentsService, 'modifySession');

Object.defineProperty(window, 'sessionStorage', {
  value: localStorageMock,
});

describe('EditSessionConfirmation', () => {
  it('No unsupported bookings, renders question to change session', () => {
    render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={0}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockSessionSummary}
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

  it('New edit session details are displayed correctly', () => {
    const newSessionStartTime = '10:15';
    const newSessionEndTime = '11:00';
    const mockNewSessionSummary: SessionSummary = {
      ...mockSessionSummary,
      ukStartDatetime: `2025-10-23T${newSessionStartTime}:00`,
      ukEndDatetime: `2025-10-23T${newSessionEndTime}:00`,
    };
    render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockNewSessionSummary}
        site="site-123"
        date="2024-06-10"
        mode="edit"
      />,
    );

    expect(
      screen.getByText(new RegExp(newSessionStartTime)),
    ).toBeInTheDocument();
    expect(screen.getByText(new RegExp(newSessionEndTime))).toBeInTheDocument();
    expect(screen.getByText('RSV Adult')).toBeInTheDocument();
    expect(screen.getByText('FLU 18-64')).toBeInTheDocument();
  });

  it('Has unsupported bookings, renders Yes/No question to cancel the appointments', () => {
    render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockSessionSummary}
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
        newlyUnsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockSessionSummary}
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
        newlyUnsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockSessionSummary}
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
  it('"No, go back" click resets decision and action to display previous step on a page ', async () => {
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockSessionSummary}
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

    const href = screen.getByText('No, go back').getAttribute('href');
    expect(href?.startsWith('/site/site-123/availability/edit?session=')).toBe(
      true,
    );
  });
});

describe('CancelSessionConfirmation', () => {
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
    sessionStorage.clear();
    jest.clearAllMocks();
  });

  it('No unsupported bookings, renders question to cancel session', () => {
    render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={0}
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
        newlyUnsupportedBookingsCount={3}
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

  it('Cancel session details are displayed correctly', () => {
    render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="cancel"
      />,
    );

    expect(screen.getByText('10:00 - 12:00')).toBeInTheDocument();
    expect(screen.getByText('RSV Adult')).toBeInTheDocument();
    expect(screen.getByText('FLU 18-64')).toBeInTheDocument();
  });

  it('Has unsupported bookings, user choose "Yes" to cancel the appointments', async () => {
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
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
    const removeItemSpy = jest.spyOn(sessionStorage, 'removeItem');
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
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

    await user.click(screen.getByRole('button', { name: 'Cancel session' }));
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(
        expect.stringContaining('newlyUnsupportedBookingsCount=3'),
      );
      expect(removeItemSpy).toHaveBeenCalledWith('availability-edit-draft');
    });
  });

  it('"No, go back" click resets decision and action to display previous step on a page ', async () => {
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
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

    const href = screen.getByText('No, go back').getAttribute('href');
    expect(
      href?.startsWith(
        '/site/site-123/view-availability/week/edit-session?date=2024-06-10&session',
      ),
    ).toBe(true);
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
    sessionStorage.clear();
    jest.clearAllMocks();
  });

  it('sends correct payload for change session and cancel appointments action', async () => {
    const newSessionStartTime = '10:15';
    const newSessionEndTime = '11:00';
    const mockNewSessionSummary: SessionSummary = {
      ...mockSessionSummary,
      capacity: 1,
      ukStartDatetime: `2025-10-23T${newSessionStartTime}:00`,
      ukEndDatetime: `2025-10-23T${newSessionEndTime}:00`,
    };
    const removeItemSpy = jest.spyOn(sessionStorage, 'removeItem');

    const mode = 'edit';
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockNewSessionSummary}
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
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '10:00',
        until: '12:00',
        services: ['RSV:Adult', 'FLU:18_64'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: {
        from: '10:15',
        until: '11:00',
        services: ['RSV:Adult', 'FLU:18_64'],
        slotLength: 10,
        capacity: 1,
      },
      newlyUnsupportedBookingAction: 'Cancel',
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
    expect(params.get('chosenAction')).toBe('cancel-appointments');
    expect(params.get('newlyUnsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('true');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');

    waitFor(() => {
      expect(removeItemSpy).toHaveBeenCalledWith('availability-edit-draft');
    });
  });

  it('sends correct payload for change session action', async () => {
    const newSessionStartTime = '10:45';
    const newSessionEndTime = '11:45';
    const mockNewSessionSummary: SessionSummary = {
      ...mockSessionSummary,
      capacity: 1,
      ukStartDatetime: `2025-10-23T${newSessionStartTime}:00`,
      ukEndDatetime: `2025-10-23T${newSessionEndTime}:00`,
    };

    const mode = 'edit';
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        newSession={mockNewSessionSummary}
        site="site-123"
        date="2024-06-10"
        mode={mode}
      />,
    );
    const removeItemSpy = jest.spyOn(sessionStorage, 'removeItem');

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
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '10:00',
        until: '12:00',
        services: ['RSV:Adult', 'FLU:18_64'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: {
        from: '10:45',
        until: '11:45',
        services: ['RSV:Adult', 'FLU:18_64'],
        slotLength: 10,
        capacity: 1,
      },
      newlyUnsupportedBookingAction: 'Orphan',
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
    expect(params.get('chosenAction')).toBe('change-session');
    expect(params.get('newlyUnsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('false');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');

    waitFor(() => {
      expect(removeItemSpy).toHaveBeenCalledWith('availability-edit-draft');
    });
  });

  it('sends correct payload for cancel session action', async () => {
    const mode = 'cancel';
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode={mode}
      />,
    );
    const removeItemSpy = jest.spyOn(sessionStorage, 'removeItem');

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
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '10:00',
        until: '12:00',
        services: ['RSV:Adult', 'FLU:18_64'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: null,
      newlyUnsupportedBookingAction: 'Orphan',
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
    expect(params.get('chosenAction')).toBe('keep-appointments');
    expect(params.get('newlyUnsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('false');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');

    waitFor(() => {
      expect(removeItemSpy).toHaveBeenCalledWith('availability-edit-draft');
    });
  });

  it('sends correct payload for cancel session and cancel appointments action', async () => {
    const mode = 'cancel';
    const { user } = render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={2}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode={mode}
      />,
    );
    const removeItemSpy = jest.spyOn(sessionStorage, 'removeItem');

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
    expect(mockModifySession).toHaveBeenCalledWith({
      from: '2024-06-10',
      to: '2024-06-10',
      site: 'site-123',
      sessionMatcher: {
        from: '10:00',
        until: '12:00',
        services: ['RSV:Adult', 'FLU:18_64'],
        slotLength: 10,
        capacity: 2,
      },
      sessionReplacement: null,
      newlyUnsupportedBookingAction: 'Cancel',
    });

    // Validate navigation
    //expect(mockPush).toHaveBeenCalledWith(
    //  expect.stringContaining(
    //    `/site/site-123/availability/${mode}/confirmed?updatedSession=undefined&date=2024-06-10&chosenAction=cancel-appointments&newlyUnsupportedBookingsCount=2&cancelAppointments=${cancelUnsupportedBookings}`,
    //  ),
    //);

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
    expect(params.get('newlyUnsupportedBookingsCount')).toBe('2');
    expect(params.get('cancelAppointments')).toBe('true');
    expect(params.get('cancelledWithoutDetailsCount')).toBe('1');

    // ensure no stray "undefined" anywhere in the URL
    expect(calledArg).not.toContain('undefined');
    waitFor(() => {
      expect(removeItemSpy).toHaveBeenCalledWith('availability-edit-draft');
    });
  });

  it('renders the correct impact note when cancelling a session', async () => {
    render(
      <SessionModificationConfirmation
        newlyUnsupportedBookingsCount={3}
        clinicalServices={mockMultipleServices}
        session={btoa(JSON.stringify(mockSessionSummary))}
        site="site-123"
        date="2024-06-10"
        mode="cancel"
      />,
    );

    expect(
      screen.getByText('Cancelling the session will affect 3 bookings.'),
    ).toBeInTheDocument();
  });
});
