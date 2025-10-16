import { useRouter } from 'next/navigation';
import { screen } from '@testing-library/react';
import CancelChoiceStep, { CancelChoiceStepProps } from './cancel-choice-step';
import render from '@testing/render';
import { InjectedWizardProps } from '@components/wizard';
import { mockSite } from '@testing/data';
import MockForm from '@testing/mockForm';
import { parseToUkDatetime } from '@services/timeService';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});

const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultProps: InjectedWizardProps & CancelChoiceStepProps = {
  date: parseToUkDatetime('2025-01-01'),
  site: mockSite,
  daySummary: {
    date: '2025-01-01',
    maximumCapacity: 10,
    totalRemainingCapacity: 7,
    totalSupportedAppointments: 3,
    totalOrphanedAppointments: 1,
    totalCancelledAppointments: 0,
    sessionSummaries: [],
  },
  clinicalServices: [],
  stepNumber: 1,
  currentStep: 1,
  isActive: true,
  setCurrentStep: mockSetCurrentStep,
  goToNextStep: mockGoToNextStep,
  goToLastStep: mockGoToLastStep,
  goToPreviousStep: mockGoToPreviousStep,
  returnRouteUponCancellation: '/',
};

describe('Cancel Day Choice Step', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      push: mockPush,
    });
    jest.clearAllMocks();
  });

  it('renders the session summary and inset text', () => {
    render(
      <MockForm submitHandler={jest.fn()}>
        <CancelChoiceStep {...defaultProps} />
      </MockForm>,
    );
    expect(
      screen.getByText('Sessions for Wednesday 1 January'),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        "4 booked appointments will be cancelled. We'll notify people that their appointment has been cancelled",
      ),
    ).toBeInTheDocument();
  });

  it('allows selecting No and navigates back on Continue', async () => {
    const { user } = render(
      <MockForm submitHandler={jest.fn()}>
        <CancelChoiceStep {...defaultProps} />
      </MockForm>,
    );

    await user.click(
      screen.getByRole('radio', {
        name: "No, I don't want to cancel the appointments",
      }),
    );
    const continueBtn = screen.getByRole('button', { name: 'Continue' });

    expect(continueBtn).toBeEnabled();

    await user.click(continueBtn);
    expect(mockPush).toHaveBeenCalledWith(
      `/site/${mockSite.id}/view-availability/week?date=2025-01-01`,
    );
  });

  it('shows a validation error when no option is selected', async () => {
    const { user } = render(
      <MockForm submitHandler={jest.fn()}>
        <CancelChoiceStep {...defaultProps} />
      </MockForm>,
    );

    const continueBtn = screen.getByRole('button', { name: 'Continue' });
    await user.click(continueBtn);

    expect(await screen.findByText('Select an option')).toBeVisible();
  });
});
