import ConfirmCancellation from './confirm-cancellation';
import { screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import render from '@testing/render';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

describe('Confirm Cancellation Page', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });

  it('renders the correct session in the table', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <ConfirmCancellation session={session} date="2025-01-15" site="TEST01" />,
    );

    expect(
      screen.getByRole('row', {
        name: '09:00 - 12:00 RSV (Adult) 2 booked 70 unbooked',
      }),
    ).toBeInTheDocument();
  });

  it('renders the radio buttons', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <ConfirmCancellation session={session} date="2025-01-15" site="TEST01" />,
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
      <ConfirmCancellation session={session} date="2025-01-15" site="TEST01" />,
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
});
