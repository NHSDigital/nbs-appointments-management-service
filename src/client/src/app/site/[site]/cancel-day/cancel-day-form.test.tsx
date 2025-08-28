import { screen } from '@testing-library/react';
import CancelDayForm from './cancel-day-form';
import render from '@testing/render';
import { useRouter } from 'next/navigation';
import { parseToUkDatetime } from '@services/timeService';
import * as appointmentsService from '@services/appointmentsService';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

jest.mock('@services/timeService', () => ({
  parseToUkDatetime: jest.fn(),
}));

jest.mock('@services/appointmentsService');
const mockCancelDay = jest.spyOn(appointmentsService, 'cancelDay');

const mockReplace = jest.fn();

const defaultProps = {
  date: '2025-01-01',
  siteId: 'site-123',
  daySummary: {
    date: '2025-01-01',
    maximumCapacity: 10,
    remainingCapacity: 7,
    bookedAppointments: 3,
    orphanedAppointments: 0,
    cancelledAppointments: 0,
    sessions: [],
  },
  clinicalServices: [],
};

describe('CancelDayForm', () => {
  beforeEach(() => {
    (useRouter as jest.Mock).mockReturnValue({ replace: mockReplace });
    (parseToUkDatetime as jest.Mock).mockReturnValue({
      format: () => 'Wednesday 1 January',
    });
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
