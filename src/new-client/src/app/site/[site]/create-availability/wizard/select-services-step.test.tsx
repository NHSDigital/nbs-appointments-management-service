import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import SelectServicesStep from './select-services-step';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockSetCurrentStep = jest.fn();

describe('Select Services Step', () => {
  it('renders', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <SelectServicesStep
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
        name: 'Create availability period Add services to your session',
      }),
    ).toBeInTheDocument;
  });

  it('permits user input', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <SelectServicesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(screen.getByRole('checkbox', { name: 'RSV' })).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: 'RSV' })).not.toBeChecked();

    await user.click(screen.getByRole('checkbox', { name: 'RSV' }));
    expect(screen.getByRole('checkbox', { name: 'RSV' })).toBeChecked();

    await user.click(screen.getByRole('checkbox', { name: 'RSV' }));
    expect(screen.getByRole('checkbox', { name: 'RSV' })).not.toBeChecked();
  });

  it('shows a validation error if no services are selected', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <SelectServicesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(screen.getByRole('checkbox', { name: 'RSV' })).not.toBeChecked();

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('At least one service must be selected'),
    ).toBeInTheDocument();
    expect(mockGoToNextStep).not.toHaveBeenCalled();
  });

  it('continues to the next step if a service is selected', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <SelectServicesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(screen.getByRole('checkbox', { name: 'RSV' })).not.toBeChecked();
    await user.click(screen.getByRole('checkbox', { name: 'RSV' }));
    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockGoToNextStep).toHaveBeenCalled();
  });
});
