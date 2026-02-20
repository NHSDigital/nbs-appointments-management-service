import { render, screen, fireEvent } from '@testing-library/react';
import BeforeYouContinueStep from './before-you-continue-step';
import { useRouter } from 'next/navigation';
import { InjectedWizardProps } from '@components/wizard';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

describe('BeforeYouContinueStep', () => {
  const mockGoToNextStep = jest.fn();
  const mockRouter = { back: jest.fn() };

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue(mockRouter);
  });

  const defaultProps: InjectedWizardProps = {
    goToNextStep: mockGoToNextStep,
    currentStep: 1,
    goToPreviousStep: jest.fn(),
    stepNumber: 1,
    isActive: true,
    setCurrentStep: jest.fn(),
    goToLastStep: jest.fn(),
    returnRouteUponCancellation: '/todo',
  };

  it('renders the base instructions when CancelADateRangeWithBookings feature flag is DISABLED', () => {
    render(
      <BeforeYouContinueStep
        {...defaultProps}
        cancelADateRangeWithBookingsEnabled={false}
      />,
    );

    expect(
      screen.getByText(/Cancel the sessions you want to change/i),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/Create new sessions with the updated details/i),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/Choose to keep existing bookings/i),
    ).not.toBeInTheDocument();
  });

  it('renders the extended instructions when CancelADateRangeWithBookings feature flag is ENABLED', () => {
    render(
      <BeforeYouContinueStep
        {...defaultProps}
        cancelADateRangeWithBookingsEnabled={true}
      />,
    );

    expect(
      screen.getByText(/Cancel the sessions you want to change/i),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/Choose to keep existing bookings/i),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/Create new sessions with the updated details/i),
    ).toBeInTheDocument();
  });

  it('calls goToNextStep when the "Continue to cancel" button is clicked', () => {
    render(
      <BeforeYouContinueStep
        {...defaultProps}
        cancelADateRangeWithBookingsEnabled={false}
      />,
    );

    const continueButton = screen.getByRole('button', {
      name: /Continue to cancel/i,
    });
    fireEvent.click(continueButton);

    expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
  });

  it('calls router.back when the BackLink is clicked', () => {
    render(
      <BeforeYouContinueStep
        {...defaultProps}
        cancelADateRangeWithBookingsEnabled={false}
      />,
    );

    const backButton = screen.getByText(/Back/i);
    fireEvent.click(backButton);

    expect(mockRouter.back).toHaveBeenCalledTimes(1);
  });
});
