import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import SummaryStep from './summary-step';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';
import { mockSingleService } from '@testing/data';

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
          clinicalServices={mockSingleService}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Add availability Check your answers',
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
          clinicalServices={mockSingleService}
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
          clinicalServices={mockSingleService}
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
          clinicalServices={mockSingleService}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Save and publish availability' }),
    );

    expect(mockOnSubmit).toHaveBeenCalledWith(currentFormState);
  });

  it('displays assurance content', async () => {
    const mockOnSubmit = jest.fn();

    render(
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
          clinicalServices={mockSingleService}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Before you continue' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'By publishing this availability, you confirm that your site is assured to deliver the services you have selected.',
      ),
    ).toBeInTheDocument();
  });
});
