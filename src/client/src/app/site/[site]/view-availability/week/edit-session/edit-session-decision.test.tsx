import { mockWeekAvailability__Summary } from '@testing/availability-and-bookings-mock-data';
import render from '@testing/render';
import { EditSessionDecision } from './edit-session-decision';
import { screen, waitFor } from '@testing-library/dom';
import { mockSite } from '@testing/data';
import { useRouter } from 'next/navigation';
import { clinicalServices } from '@types';

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
        clinicalServices={clinicalServices}
        multipleServicesEnabled={false}
      />,
    );

    expect(
      screen.getByRole('row', {
        name: '09:00 - 12:00 RSV Adult 2 booked 70 unbooked',
      }),
    ).toBeInTheDocument();
  });

  it.each([false, true])(
    'renders 3 radio buttons when multiple services',
    (multipleServicesEnabled: boolean) => {
      const session = btoa(
        JSON.stringify(mockWeekAvailability__Summary[1].sessions[0]),
      );
      render(
        <EditSessionDecision
          sessionSummary={session}
          date="2025-01-15"
          site={mockSite}
          clinicalServices={clinicalServices}
          multipleServicesEnabled={multipleServicesEnabled}
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

      if (multipleServicesEnabled) {
        expect(
          screen.queryByRole('radio', {
            name: 'Remove services from this session',
          }),
        ).toBeInTheDocument();
      } else {
        expect(
          screen.queryByRole('radio', {
            name: 'Remove services from this session',
          }),
        ).not.toBeInTheDocument();
      }
    },
  );

  it('does not render the reduce services radio button when only one service in the session, despite MultipleServices enabled', () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={clinicalServices}
        multipleServicesEnabled
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

    expect(
      screen.queryByRole('radio', {
        name: 'Remove services from this session',
      }),
    ).not.toBeInTheDocument();
  });

  it.each([false, true])(
    'toggles between the radio buttons',
    async (multipleServicesEnabled: boolean) => {
      const session = btoa(
        JSON.stringify(mockWeekAvailability__Summary[1].sessions[0]),
      );
      const { user } = render(
        <EditSessionDecision
          sessionSummary={session}
          date="2025-01-15"
          site={mockSite}
          clinicalServices={clinicalServices}
          multipleServicesEnabled={multipleServicesEnabled}
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

      if (multipleServicesEnabled) {
        expect(
          screen.getByRole('radio', {
            name: 'Remove services from this session',
          }),
        ).not.toBeChecked();
      }

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

      if (multipleServicesEnabled) {
        expect(
          screen.getByRole('radio', {
            name: 'Remove services from this session',
          }),
        ).not.toBeChecked();

        await user.click(
          screen.getByRole('radio', {
            name: 'Remove services from this session',
          }),
        );

        expect(
          screen.getByRole('radio', {
            name: 'Remove services from this session',
          }),
        ).toBeChecked();

        expect(
          screen.getByRole('radio', {
            name: 'Change the length or capacity of this session',
          }),
        ).not.toBeChecked();

        expect(
          screen.getByRole('radio', { name: 'Cancel this session' }),
        ).not.toBeChecked();
      }
    },
  );

  it('displays a validation error if no value is selected', async () => {
    const session = btoa(
      JSON.stringify(mockWeekAvailability__Summary[0].sessions[0]),
    );
    const { user } = render(
      <EditSessionDecision
        sessionSummary={session}
        date="2025-01-15"
        site={mockSite}
        clinicalServices={clinicalServices}
        multipleServicesEnabled={false}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Select an option')).toBeInTheDocument();

    expect(mockPush).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('radio', {
        name: 'Cancel this session',
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
        clinicalServices={clinicalServices}
        multipleServicesEnabled={false}
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
});
