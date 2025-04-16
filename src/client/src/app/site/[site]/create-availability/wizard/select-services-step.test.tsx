import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import SelectServicesStep from './select-services-step';
import { clinicalServices } from '@types';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

describe('Select Services Step', () => {
  it('renders', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ session: { services: [] } }}
      >
        <SelectServicesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Create weekly session Add services to your session',
      }),
    ).toBeInTheDocument();
  });

  it('permits user input', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ session: { services: [] } }}
      >
        <SelectServicesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('checkbox', { name: 'RSV (Adult)' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('checkbox', { name: 'RSV (Adult)' }),
    ).not.toBeChecked();

    await user.click(screen.getByRole('checkbox', { name: 'RSV (Adult)' }));
    expect(screen.getByRole('checkbox', { name: 'RSV (Adult)' })).toBeChecked();

    await user.click(screen.getByRole('checkbox', { name: 'RSV (Adult)' }));
    expect(
      screen.getByRole('checkbox', { name: 'RSV (Adult)' }),
    ).not.toBeChecked();
  });

  it('shows a validation error if no services are selected', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ session: { services: [] } }}
      >
        <SelectServicesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('checkbox', { name: 'RSV (Adult)' }),
    ).not.toBeChecked();

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Select a service')).toBeInTheDocument();

    expect(mockGoToNextStep).not.toHaveBeenCalled();
  });

  it('continues to the next step if a service is selected', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ session: { services: [] } }}
      >
        <SelectServicesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          clinicalServices={clinicalServices}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('checkbox', { name: 'RSV (Adult)' }),
    ).not.toBeChecked();
    await user.click(screen.getByRole('checkbox', { name: 'RSV (Adult)' }));
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockGoToNextStep).toHaveBeenCalled();
  });
});
