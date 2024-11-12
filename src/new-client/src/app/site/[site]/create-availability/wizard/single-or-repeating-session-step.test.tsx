import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import SingleOrRepeatingSessionStep from './single-or-repeating-session-step';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

describe('Single or Repeating Session Step', () => {
  it('renders', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <SingleOrRepeatingSessionStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Create availability period What type of session do you want to create?',
      }),
    ).toBeInTheDocument;
  });

  it('toggles between single and repeat', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <SingleOrRepeatingSessionStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    await user.click(screen.getByRole('radio', { name: 'Repeat session' }));
    expect(screen.getByRole('radio', { name: 'Repeat session' })).toBeChecked();
    expect(
      screen.getByRole('radio', { name: 'Single session' }),
    ).not.toBeChecked();

    await user.click(screen.getByRole('radio', { name: 'Single session' }));
    expect(
      screen.getByRole('radio', { name: 'Repeat session' }),
    ).not.toBeChecked();
    expect(screen.getByRole('radio', { name: 'Single session' })).toBeChecked();
  });
});
