import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import SummaryStep from './summary-step';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';
import { ClinicalService } from '@types';

const clinicalServices: ClinicalService[] = [
  { label: 'RSV Adult', value: 'RSV:Adult' },
];

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const currentFormState: CreateAvailabilityFormValues = {
  days: ['Tuesday', 'Thursday'],
  sessionType: 'single',
  session: {
    startTime: {
      hour: 9,
      minute: 30,
    },
    endTime: {
      hour: 17,
      minute: 45,
    },
    break: 'no',
    capacity: 2,
    slotLength: 15,
    services: ['RSV:Adult'],
  },
  startDate: {
    day: 28,
    month: 2,
    year: 2027,
  },
  endDate: {
    day: 0,
    month: 0,
    year: 0,
  },
};

describe('Summary Step', () => {
  it('renders', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={currentFormState}
      >
        <SummaryStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Check single date session',
      }),
    ).toBeInTheDocument();
  });

  it('summarises the single date form data collected by the wizard', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={currentFormState}
      >
        <SummaryStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    verifySummaryListItem('Date', '28 February 2027');
    verifySummaryListItem('Time', '09:30 - 17:45');
    verifySummaryListItem('Vaccinators or vaccination spaces available', '2');
    verifySummaryListItem('Appointment length', '15 minutes');
    verifySummaryListItem('Services available', 'RSV Adult');

    expect(
      screen.queryByRole('term', { name: 'Days' }),
    ).not.toBeInTheDocument();
  });

  it('summarises repeating sessions form data collected by the wizard', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...currentFormState,
          sessionType: 'repeating',
          endDate: { day: 1, month: 3, year: 2028 },
        }}
      >
        <SummaryStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    verifySummaryListItem('Dates', '28 February 2027 - 1 March 2028');
  });

  it('displays a Save button which submits the form', async () => {
    const mockOnSubmit = jest.fn();

    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={mockOnSubmit}
        defaultValues={currentFormState}
      >
        <SummaryStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Save session' }));

    expect(mockOnSubmit).toHaveBeenCalledWith(currentFormState);
  });

  it('hides the appointments per hour calculation if session length is under 1 hour', () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...currentFormState,
          session: {
            ...currentFormState.session,
            startTime: {
              hour: 9,
              minute: 0,
            },
            endTime: {
              hour: 9,
              minute: 30,
            },
          },
        }}
      >
        <SummaryStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    expect(screen.getByText('4')).toBeInTheDocument();
    expect(
      screen.getByText(/total appointments in the session/),
    ).toBeInTheDocument();

    expect(screen.queryByText(/Up to/)).not.toBeInTheDocument();
    expect(screen.queryByText('8')).not.toBeInTheDocument();
    expect(screen.queryByText(/appointments per hour/)).not.toBeInTheDocument();
  });
});
