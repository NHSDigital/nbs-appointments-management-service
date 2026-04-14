import { render, screen, fireEvent, act } from '@testing-library/react';
import BeforeYouContinueStep from './before-you-continue-step';
import { useRouter } from 'next/navigation';
import { InjectedWizardProps } from '@components/wizard';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

describe('BeforeYouContinueStep', () => {
  const mockGoToNextStep = jest.fn();
  const mockRouter = {
    back: jest.fn(),
    push: jest.fn(),
  };

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

  it('renders the Back link with the correct role', () => {
    render(
      <BeforeYouContinueStep
        {...defaultProps}
        cancelADateRangeWithBookingsEnabled={false}
      />,
    );

    const backLink = screen.getByRole('link', { name: /Back/i });
    expect(backLink).toBeInTheDocument();
  });

  it('calls router.push with the previousUrl when one is provided', () => {
    const customUrl = '/specific-previous-page';

    render(
      <BeforeYouContinueStep
        {...defaultProps}
        cancelADateRangeWithBookingsEnabled={false}
        previousUrl={customUrl}
      />,
    );

    const backLink = screen.getByText(/Back/i);
    fireEvent.click(backLink);

    // This proves the 'if (previousUrl)' logic works
    expect(mockRouter.push).toHaveBeenCalledWith(customUrl);
  });

  it('calls router.push with "/sites" fallback when previousUrl is missing', () => {
    render(
      <BeforeYouContinueStep
        {...defaultProps}
        cancelADateRangeWithBookingsEnabled={false}
        previousUrl={undefined} // No previous URL
      />,
    );

    const backLink = screen.getByText(/Back/i);
    fireEvent.click(backLink);

    // External traffic fallback
    expect(mockRouter.push).toHaveBeenCalledWith('/sites');
  });
});
