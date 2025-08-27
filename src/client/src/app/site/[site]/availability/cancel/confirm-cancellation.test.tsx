import ConfirmCancellation from './confirm-cancellation';
import { screen, waitFor } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import render from '@testing/render';
import { cancelSession } from '@services/appointmentsService';
import { ClinicalService } from '@types';

const clinicalServices: ClinicalService[] = [
  { label: 'RSV Adult', value: 'RSV:Adult' },
];

jest.mock('@services/appointmentsService');
const mockCancelSession = cancelSession as jest.Mock;

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

describe('Confirm Cancellation Page', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      push: mockPush,
    });
  });

  it('renders the correct session in the table', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <ConfirmCancellation
        session={session}
        date="2025-01-15"
        site="TEST01"
        clinicalServices={clinicalServices}
      />,
    );

    expect(
      screen.getByRole('row', {
        name: '09:00 - 12:00 RSV Adult 2 booked 70 unbooked',
      }),
    ).toBeInTheDocument();
  });

  it('renders the radio buttons', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <ConfirmCancellation
        session={session}
        date="2025-01-15"
        site="TEST01"
        clinicalServices={clinicalServices}
      />,
    );

    expect(
      screen.getByRole('radio', { name: 'Yes, I want to cancel this session' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', {
        name: "No, I don't want to cancel this session",
      }),
    ).toBeInTheDocument();
  });

  it('toggles between yes and no', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <ConfirmCancellation
        session={session}
        date="2025-01-15"
        site="TEST01"
        clinicalServices={clinicalServices}
      />,
    );

    await user.click(
      screen.getByRole('radio', { name: 'Yes, I want to cancel this session' }),
    );
    expect(
      screen.getByRole('radio', { name: 'Yes, I want to cancel this session' }),
    ).toBeChecked();
    expect(
      screen.getByRole('radio', {
        name: "No, I don't want to cancel this session",
      }),
    ).not.toBeChecked();

    await user.click(
      screen.getByRole('radio', {
        name: "No, I don't want to cancel this session",
      }),
    );
    expect(
      screen.getByRole('radio', { name: 'Yes, I want to cancel this session' }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('radio', {
        name: "No, I don't want to cancel this session",
      }),
    ).toBeChecked();
  });

  it('displays a validation error if no value is selected', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <ConfirmCancellation
        session={session}
        date="2025-01-15"
        site="TEST01"
        clinicalServices={clinicalServices}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Select an option')).toBeInTheDocument();

    expect(mockPush).not.toHaveBeenCalled();
    expect(mockCancelSession).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('radio', { name: 'Yes, I want to cancel this session' }),
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    waitFor(() => {
      expect(mockCancelSession).toHaveBeenCalled();
      expect(mockPush).toHaveBeenCalled();
    });
  });

  it('submits the form and cancels the session', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <ConfirmCancellation
        session={session}
        date="2025-01-15"
        site="TEST01"
        clinicalServices={clinicalServices}
      />,
    );

    await user.click(
      screen.getByRole('radio', { name: 'Yes, I want to cancel this session' }),
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    waitFor(() => {
      expect(mockCancelSession).toHaveBeenCalled();
      expect(mockPush).toHaveBeenCalledWith(
        `/site/TEST01/view-availability/week/edit-session?session=${session}&date=2025-01-15`,
      );
    });
  });

  it('navigates to the change session route when user decides not to cancel the session', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <ConfirmCancellation
        session={session}
        date="2025-01-15"
        site="TEST01"
        clinicalServices={clinicalServices}
      />,
    );

    await user.click(
      screen.getByRole('radio', {
        name: "No, I don't want to cancel this session",
      }),
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    waitFor(() => {
      expect(mockCancelSession).not.toHaveBeenCalled();
      expect(mockPush).toHaveBeenCalledWith(
        `/site/TEST01/view-availability/week/edit-session?session=${session}&date=2025-01-15`,
      );
    });
  });
});
