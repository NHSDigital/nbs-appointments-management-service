import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import render from '@testing/render';
import { EditSessionDecision } from './edit-session-decision';
import { screen, waitFor } from '@testing-library/dom';
import {
  mockMultipleServices,
  mockSingleService,
  mockSite,
} from '@testing/data';
import { useRouter } from 'next/navigation';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

describe('Edit Session Decision Page', () => {
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
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockSingleService}
        cancelSessionUpliftedJourneyFlag={false}
      />,
    );

    expect(
      screen.getByRole('row', {
        name: '09:00 - 12:00 RSV Adult 2 booked',
      }),
    ).toBeInTheDocument();
  });

  it('renders 3 radio buttons', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[1].sessions[0]),
    );
    render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockMultipleServices}
        cancelSessionUpliftedJourneyFlag={false}
      />,
    );

    expect(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', { name: 'Cancel the session' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('radio', {
        name: 'Remove a service or multiple services',
      }),
    ).toBeInTheDocument();
  });

  it('does not render the reduce services radio button when only one service in the session', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockSingleService}
        cancelSessionUpliftedJourneyFlag={false}
      />,
    );

    expect(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', { name: 'Cancel the session' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('radio', {
        name: 'Remove a service or multiple services',
      }),
    ).not.toBeInTheDocument();
  });

  it('toggles between the radio buttons', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[1].sessions[0]),
    );
    const { user } = render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockSingleService}
        cancelSessionUpliftedJourneyFlag={false}
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
      screen.getByRole('radio', { name: 'Cancel the session' }),
    ).not.toBeChecked();

    expect(
      screen.getByRole('radio', {
        name: 'Remove a service or multiple services',
      }),
    ).not.toBeChecked();

    await user.click(screen.getByRole('radio', { name: 'Cancel the session' }));

    expect(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    ).not.toBeChecked();

    expect(
      screen.getByRole('radio', { name: 'Cancel the session' }),
    ).toBeChecked();

    expect(
      screen.getByRole('radio', {
        name: 'Remove a service or multiple services',
      }),
    ).not.toBeChecked();

    await user.click(
      screen.getByRole('radio', {
        name: 'Remove a service or multiple services',
      }),
    );

    expect(
      screen.getByRole('radio', {
        name: 'Remove a service or multiple services',
      }),
    ).toBeChecked();

    expect(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    ).not.toBeChecked();

    expect(
      screen.getByRole('radio', { name: 'Cancel the session' }),
    ).not.toBeChecked();
  });

  it('displays a validation error if no value is selected', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockSingleService}
        cancelSessionUpliftedJourneyFlag={false}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Select an option')).toBeInTheDocument();

    expect(mockPush).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('radio', {
        name: 'Cancel the session',
      }),
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    waitFor(() => {
      expect(mockPush).toHaveBeenCalled();
    });
  });

  it('submits the form', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockSingleService}
        cancelSessionUpliftedJourneyFlag={false}
      />,
    );

    await user.click(
      screen.getByRole('radio', {
        name: 'Change the length or capacity of this session',
      }),
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    waitFor(() => {
      expect(mockPush).toHaveBeenCalled();
    });
  });

  it('reroutes to cancel/confirmation when cancelSessionUpliftedJourneyFlag is true', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockSingleService}
        cancelSessionUpliftedJourneyFlag={true}
      />,
    );

    await user.click(
      screen.getByRole('radio', {
        name: /cancel the session/i,
      }),
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(
        expect.stringContaining(
          `/site/${mockSite.id}/availability/cancel/confirmation?session=${session}&date=2025-01-15`,
        ),
      );
    });
  });

  it('reroutes to cancel when cancelSessionUpliftedJourneyFlag is false', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={mockSingleService}
        cancelSessionUpliftedJourneyFlag={false}
      />,
    );

    await user.click(
      screen.getByRole('radio', {
        name: /cancel the session/i,
      }),
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(
        expect.stringContaining(
          `/site/${mockSite.id}/availability/cancel?session=${session}&date=2025-01-15`,
        ),
      );
    });
  });
});
