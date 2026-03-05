import render from '@testing/render';
import { screen } from '@testing-library/react';
import ConfirmationStep from './confirmation-step';
import MockForm from '@testing/mockForm';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';

const siteID = 'site-123';
const defaultProps = {
  site: siteID,
  currentStep: 4,
  stepNumber: 4,
  isActive: true,
  goToNextStep: jest.fn(),
  goToPreviousStep: jest.fn(),
  setCurrentStep: jest.fn(),
  goToLastStep: jest.fn(),
  returnRouteUponCancellation: '',
};

const renderComponent = (
  cancellationSummary?: ChangeAvailabilityFormValues['cancellationSummary'],
) => {
  const defaultValues = {
    cancellationSummary,
  } as ChangeAvailabilityFormValues;

  return render(
    <MockForm<ChangeAvailabilityFormValues>
      defaultValues={defaultValues}
      submitHandler={jest.fn()}
    >
      <ConfirmationStep {...defaultProps} />
    </MockForm>,
  );
};

describe('ConfirmationStep', () => {
  it('renders the correct heading for multiple cancelled sessions', () => {
    renderComponent({
      cancelledSessionsCount: 5,
      cancelledBookingsCount: 0,
      bookingsWithoutContactDetailsCount: 0,
    });

    expect(
      screen.getByRole('heading', { name: /5 sessions cancelled/i }),
    ).toBeInTheDocument();
  });

  it('renders the correct heading for a single cancelled session', () => {
    renderComponent({
      cancelledSessionsCount: 1,
      cancelledBookingsCount: 0,
      bookingsWithoutContactDetailsCount: 0,
    });

    expect(
      screen.getByRole('heading', { name: /1 session cancelled/i }),
    ).toBeInTheDocument();
  });

  it('renders the navigation links with the correct site ID', () => {
    renderComponent({
      cancelledSessionsCount: 1,
      cancelledBookingsCount: 0,
      bookingsWithoutContactDetailsCount: 0,
    });

    const createLink = screen.getByRole('link', {
      name: /create availability/i,
    });
    const backToViewLink = screen.getByRole('link', {
      name: /go back to view availability/i,
    });

    expect(createLink).toHaveAttribute(
      'href',
      `/site/${siteID}/create-availability/wizard`,
    );
    expect(backToViewLink).toHaveAttribute(
      'href',
      `/site/${siteID}/view-availability`,
    );
  });
});
