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

describe('CancellationImpactStep', () => {
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
