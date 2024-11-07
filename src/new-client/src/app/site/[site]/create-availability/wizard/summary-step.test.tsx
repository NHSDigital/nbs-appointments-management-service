import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import SummaryStep from './summary-step';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
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
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Create availability period Check availability period',
      }),
    ).toBeInTheDocument;
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
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(screen.getByRole('term', { name: 'Date' })).toBeInTheDocument;
    expect(screen.getByRole('definition', { name: '28 February 2027' }))
      .toBeInTheDocument;

    expect(screen.queryByRole('term', { name: 'Days' })).not.toBeInTheDocument;

    expect(screen.getByRole('term', { name: 'Time' })).toBeInTheDocument;
    expect(screen.getByRole('definition', { name: '09:30 - 17:45' }))
      .toBeInTheDocument;

    expect(screen.getByRole('term', { name: 'Services available' }))
      .toBeInTheDocument;
    expect(
      screen.getByRole('definition', {
        name: 'RSV (Adult)',
      }),
    ).toBeInTheDocument;

    expect(
      screen.getByRole('term', { name: 'Maximum simultaneous appointments' }),
    ).toBeInTheDocument;
    expect(
      screen.getByRole('definition', {
        name: '2',
      }),
    ).toBeInTheDocument;

    expect(screen.getByRole('term', { name: 'Appointment length in minutes' }))
      .toBeInTheDocument;
    expect(
      screen.getByRole('definition', {
        name: '15',
      }),
    ).toBeInTheDocument;
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
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(screen.getByRole('term', { name: 'Dates' })).toBeInTheDocument;
    expect(
      screen.getByRole('definition', {
        name: '28 February 2027 - 1 March 2028',
      }),
    ).toBeInTheDocument;
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
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(mockOnSubmit).toHaveBeenCalledWith(currentFormState);
  });
});
