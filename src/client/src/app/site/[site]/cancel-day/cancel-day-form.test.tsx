import { render, screen, fireEvent } from '@testing-library/react';
import CancelDayForm from './cancel-day-form';
import { useRouter } from 'next/navigation';
import { parseToUkDatetime } from '@services/timeService';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

jest.mock('@services/timeService', () => ({
  parseToUkDatetime: jest.fn(),
}));

jest.mock('@components/session-summary-table', () => ({
  SessionSummaryTable: ({ tableCaption }: { tableCaption: string }) => (
    <div data-testid="session-summary">{tableCaption}</div>
  ),
}));

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
    expect(screen.getByTestId('session-summary')).toHaveTextContent(
      'Sessions for Wednesday 1 January',
    );
    expect(
      screen.getByText(/3 booked appointments will be cancelled/i),
    ).toBeInTheDocument();
  });

  it('initially disables Continue until a choice is made', () => {
    render(<CancelDayForm {...defaultProps} />);
    expect(screen.getByRole('button', { name: /continue/i })).toBeDisabled();
  });

  it('allows selecting No and navigates back on Continue', () => {
    render(<CancelDayForm {...defaultProps} />);
    fireEvent.click(screen.getByLabelText(/no, i don't want/i));
    const continueBtn = screen.getByRole('button', { name: /continue/i });
    expect(continueBtn).toBeEnabled();
    fireEvent.click(continueBtn);
    expect(mockReplace).toHaveBeenCalledWith(
      `/site/site-123/view-availability/week?date=2025-01-01`,
    );
  });

  it('allows selecting Yes and shows confirmation step', () => {
    render(<CancelDayForm {...defaultProps} />);
    fireEvent.click(screen.getByLabelText(/yes, i want to cancel/i));
    fireEvent.click(screen.getByRole('button', { name: /continue/i }));
    expect(
      screen.getByRole('button', { name: /cancel day/i }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /no, go back/i })).toHaveAttribute(
      'href',
      '/site/site-123/view-availability/week?date=2025-01-01',
    );
  });

  it('calls handleCancel when clicking Cancel day', () => {
    const consoleSpy = jest.spyOn(console, 'log').mockImplementation();
    render(<CancelDayForm {...defaultProps} />);
    fireEvent.click(screen.getByLabelText(/yes, i want to cancel/i));
    fireEvent.click(screen.getByRole('button', { name: /continue/i }));
    fireEvent.click(screen.getByRole('button', { name: /cancel day/i }));
    expect(consoleSpy).toHaveBeenCalledWith('Day cancelled!');
  });
});
