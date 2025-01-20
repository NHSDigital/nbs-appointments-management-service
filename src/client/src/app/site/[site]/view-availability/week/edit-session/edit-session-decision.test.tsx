import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import render from '@testing/render';
import { EditSessionDecision } from './edit-session-decision';
import { screen } from '@testing-library/dom';
import { mockSite } from '@testing/data';
import { useRouter } from 'next/navigation';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

describe('Edit Session Decision Page', () => {
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
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
      />,
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
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
      />,
    );

    expect(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', { name: 'Cancel this session' }),
    ).toBeInTheDocument();
  });

  it('toggles between edit and cancel', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
      />,
    );

    await user.click(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    );
    expect(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    ).toBeChecked();
    expect(
      screen.getByRole('radio', { name: 'Cancel this session' }),
    ).not.toBeChecked();

    await user.click(
      screen.getByRole('radio', { name: 'Cancel this session' }),
    );
    expect(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('radio', { name: 'Cancel this session' }),
    ).toBeChecked();
  });
});
