import { screen } from '@testing-library/react';
import CancelDayForm from './cancel-day-form';
import render from '@testing/render';
import { useRouter } from 'next/navigation';
import * as appointmentsService from '@services/appointmentsService';
import { mockCancelDayResponse } from '@testing/data';
import asServerActionResult from '@testing/asServerActionResult';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});

jest.mock('@services/appointmentsService');
const mockCancelDay = jest.spyOn(appointmentsService, 'cancelDay');

const mockReplace = jest.fn();

const defaultProps = {
  date: '2025-01-01',
  siteId: 'site-123',
  daySummary: {
    date: '2025-01-01',
    maximumCapacity: 10,
    totalRemainingCapacity: 7,
    totalSupportedAppointments: 3,
    totalOrphanedAppointments: 0,
    totalCancelledAppointments: 0,
    sessionSummaries: [],
  },
  clinicalServices: [],
};

describe('CancelDayForm', () => {
  beforeEach(() => {
    (useRouter as jest.Mock).mockReturnValue({ replace: mockReplace });
    mockCancelDay.mockResolvedValue(
      asServerActionResult(mockCancelDayResponse),
    );
    jest.clearAllMocks();
  });

  it('renders the session summary and inset text', () => {
    render(<CancelDayForm {...defaultProps} />);
    expect(
      screen.getByText('Sessions for Wednesday 1 January'),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        "3 booked appointments will be cancelled. We'll notify people that their appointment has been cancelled",
      ),
    ).toBeInTheDocument();
  });

  it('allows selecting No and navigates back on Continue', async () => {
    const { user } = render(<CancelDayForm {...defaultProps} />);

    await user.click(
      screen.getByRole('radio', {
        name: "No, I don't want to cancel the appointments",
      }),
    );
    const continueBtn = screen.getByRole('button', { name: 'Continue' });

    expect(continueBtn).toBeEnabled();

    await user.click(continueBtn);
    expect(mockReplace).toHaveBeenCalledWith(
      `/site/site-123/view-availability/week?date=2025-01-01`,
    );
  });

  it('allows selecting Yes and shows confirmation step', async () => {
    const { user } = render(<CancelDayForm {...defaultProps} />);

    await user.click(
      screen.getByRole('radio', {
        name: 'Yes, I want to cancel the appointments',
      }),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByRole('button', { name: 'Cancel day' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'No, go back' })).toHaveAttribute(
      'href',
      '/site/site-123/view-availability/week?date=2025-01-01',
    );
  });

  it('calls handleCancel when clicking Cancel day', async () => {
    const { user } = render(<CancelDayForm {...defaultProps} />);

    await user.click(
      screen.getByRole('radio', {
        name: 'Yes, I want to cancel the appointments',
      }),
    );
    await user.click(screen.getByRole('button', { name: 'Continue' }));
    await user.click(screen.getByRole('button', { name: 'Cancel day' }));

    expect(mockCancelDay).toHaveBeenCalled();
  });
});
