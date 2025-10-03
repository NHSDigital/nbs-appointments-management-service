import { InjectedWizardProps } from '@components/wizard';
import { mockSite } from '@testing/data';
import { CancelChoiceStepProps } from './cancel-choice-step';
import { screen } from '@testing-library/react';
import { ConfirmCancelChoiceStep } from './confirm-cancel-choice-step';
import MockForm from '@testing/mockForm';
import render from '@testing/render';

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

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultProps: InjectedWizardProps & CancelChoiceStepProps = {
  date: '2025-01-01',
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

describe('Confirm cancel day step', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders', () => {
    render(
      <MockForm submitHandler={jest.fn()}>
        <ConfirmCancelChoiceStep {...defaultProps} />
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
    expect(
      screen.getByRole('button', { name: 'Cancel day' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'No, go back' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'No, go back' })).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/view-availability/week?date=2025-01-01`,
    );
  });
});
