import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import DaysOfWeekStep from './days-of-week-step';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockSetCurrentStep = jest.fn();

describe('Days of Week Step', () => {
  it('renders', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ days: [] }}
      >
        <DaysOfWeekStep
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
        name: 'Create availability period Select days that you want to add to your availability period',
      }),
    ).toBeInTheDocument;
  });

  it('permits user input', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ days: [] }}
      >
        <DaysOfWeekStep
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
      screen.getByRole('checkbox', { name: 'Monday' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: 'Monday' })).not.toBeChecked();

    await user.click(screen.getByRole('checkbox', { name: 'Monday' }));
    expect(screen.getByRole('checkbox', { name: 'Monday' })).toBeChecked();

    await user.click(screen.getByRole('checkbox', { name: 'Monday' }));
    expect(screen.getByRole('checkbox', { name: 'Monday' })).not.toBeChecked();
  });

  it('shows a validation error if no days are selected', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ days: [] }}
      >
        <DaysOfWeekStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('Services must run on at least one day'),
    ).toBeInTheDocument();
    expect(mockGoToNextStep).not.toHaveBeenCalled();
  });

  it('continues to the next step if a days is selected', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ days: [] }}
      >
        <DaysOfWeekStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(screen.getByRole('checkbox', { name: 'Monday' })).not.toBeChecked();
    await user.click(screen.getByRole('checkbox', { name: 'Monday' }));
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockGoToNextStep).toHaveBeenCalled();
  });
});
