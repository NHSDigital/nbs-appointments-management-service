import render from '@testing/render';
import { screen } from '@testing-library/react';
import CancellationImpactStep from './cancellation-impact-step';
import { useRouter } from 'next/navigation';
import MockForm from '@testing/mockForm';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

const mockRouterPush = jest.fn();
const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();

const defaultProps = {
  cancelADateRangeWithBookingsEnabled: false,
  site: 'test-site-123',
  goToNextStep: mockGoToNextStep,
  goToPreviousStep: mockGoToPreviousStep,
  currentStep: 2,
  stepNumber: 2,
  isActive: true,
  setCurrentStep: jest.fn(),
  goToLastStep: jest.fn(),
  returnRouteUponCancellation: '/some-return-path',
};

const defaultFormValues: ChangeAvailabilityFormValues = {
  startDate: { day: '01', month: '01', year: '2026' },
  endDate: { day: '02', month: '01', year: '2026' },
};

describe('When bookings cancellation is DISABLED', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
      push: mockRouterPush,
    });
  });

  const renderComponent = (props = defaultProps) => {
    return render(
      <MockForm<ChangeAvailabilityFormValues>
        defaultValues={defaultFormValues}
        submitHandler={jest.fn()}
      >
        <CancellationImpactStep {...props} />
      </MockForm>,
    );
  };

  describe('When bookings cancellation is NOT enabled', () => {
    it('renders the "Cannot cancel" heading and body text', () => {
      renderComponent(defaultProps);

      expect(
        screen.getByText('You cannot cancel these sessions'),
      ).toBeInTheDocument();
      expect(
        screen.getByText(/There are existing bookings for these sessions/i),
      ).toBeInTheDocument();
    });

    it('navigates back to view availability when the return button is clicked', async () => {
      const { user } = renderComponent({
        ...defaultProps,
        cancelADateRangeWithBookingsEnabled: false,
      });

      const returnBtn = screen.getByRole('button', {
        name: /Return to view availability/i,
      });
      await user.click(returnBtn);

      expect(mockRouterPush).toHaveBeenCalledWith(
        `/site/${defaultProps.site}/view-availability`,
      );
    });

    it('calls goToPreviousStep when "Select different dates" is clicked', async () => {
      const { user } = renderComponent({
        ...defaultProps,
        cancelADateRangeWithBookingsEnabled: false,
      });

      const backBtn = screen.getByRole('button', {
        name: /Select different dates/i,
      });
      await user.click(backBtn);

      expect(mockGoToPreviousStep).toHaveBeenCalledTimes(1);
    });

    it('calls goToPreviousStep when the NHS BackLink is clicked', async () => {
      const { user } = renderComponent();

      const backLink = screen.getByText('Back');
      await user.click(backLink);

      expect(mockGoToPreviousStep).toHaveBeenCalledTimes(1);
    });
  });
});

describe('When bookings cancellation is ENABLED', () => {
  const enabledProps = {
    ...defaultProps,
    cancelADateRangeWithBookingsEnabled: true,
  };

  const renderWithSummary = (sessionCount: number, bookingCount: number) => {
    const customValues: ChangeAvailabilityFormValues = {
      ...defaultFormValues,
      proposedCancellationSummary: { sessionCount, bookingCount },
    };

    return render(
      <MockForm<ChangeAvailabilityFormValues>
        defaultValues={customValues}
        submitHandler={jest.fn()}
      >
        <CancellationImpactStep {...enabledProps} />
      </MockForm>,
    );
  };

  describe('renderNoSessions (sessionCount is 0)', () => {
    it('renders the no sessions message and heading', () => {
      renderWithSummary(0, 0);

      expect(
        screen.getByText('There are no sessions in this date range'),
      ).toBeInTheDocument();
      expect(
        screen.getByText('You should choose a new date range.'),
      ).toBeInTheDocument();
    });

    it('calls goToPreviousStep when "Choose a new date range" is clicked', async () => {
      const { user } = renderWithSummary(0, 0);

      const btn = screen.getByRole('button', {
        name: /Choose a new date range/i,
      });
      await user.click(btn);

      expect(mockGoToPreviousStep).toHaveBeenCalledTimes(1);
    });
  });

  describe('renderNoBookings (bookingCount is 0)', () => {
    it('renders plural text correctly when multiple sessions have no bookings', () => {
      renderWithSummary(5, 0);

      expect(
        screen.getByText('You are about to cancel 5 sessions'),
      ).toBeInTheDocument();
      expect(
        screen.getByText(/There are no bookings for these sessions/i),
      ).toBeInTheDocument();
    });

    it('renders singular text correctly when one session has no bookings', () => {
      renderWithSummary(1, 0);

      expect(
        screen.getByText('You are about to cancel 1 session'),
      ).toBeInTheDocument();
      expect(
        screen.getByText(/There are no bookings for this session/i),
      ).toBeInTheDocument();
    });

    it('calls goToNextStep when "Continue" is clicked', async () => {
      const { user } = renderWithSummary(3, 0);

      const continueBtn = screen.getByRole('button', { name: /Continue/i });
      await user.click(continueBtn);

      expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
    });
  });
});
