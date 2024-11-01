import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import TimeAndCapacityStep from './time-and-capacity-step';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockSetCurrentStep = jest.fn();

describe('Time and Capacity Step', () => {
  it('renders', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          session: {
            startTime: {
              hour: 9,
              minute: 0,
            },
            endTime: {
              hour: 17,
              minute: 0,
            },
          },
        }}
      >
        <TimeAndCapacityStep
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
        name: 'Create availability period Set time and capacity for your session',
      }),
    ).toBeInTheDocument;
  });

  it('permits start and end time data entry', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          session: {
            startTime: {
              hour: 9,
              minute: 30,
            },
            endTime: {
              hour: 17,
              minute: 45,
            },
          },
        }}
      >
        <TimeAndCapacityStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    const startTimeHourInput = screen.getByRole('textbox', {
      name: 'Session start time - hour',
    });
    const startTimeMinuteInput = screen.getByRole('textbox', {
      name: 'Session start time - minute',
    });
    const endTimeHourInput = screen.getByRole('textbox', {
      name: 'Session end time - hour',
    });
    const endTimeMinuteInput = screen.getByRole('textbox', {
      name: 'Session end time - minute',
    });

    expect(startTimeHourInput).toHaveDisplayValue('9');
    await user.clear(startTimeHourInput);
    await user.type(startTimeHourInput, '12');
    expect(startTimeHourInput).toHaveDisplayValue('12');

    expect(startTimeMinuteInput).toHaveDisplayValue('30');
    await user.clear(startTimeMinuteInput);
    await user.type(startTimeMinuteInput, '17');
    expect(startTimeMinuteInput).toHaveDisplayValue('17');

    expect(endTimeHourInput).toHaveDisplayValue('17');
    await user.clear(endTimeHourInput);
    await user.type(endTimeHourInput, '6');
    expect(endTimeHourInput).toHaveDisplayValue('6');

    expect(endTimeMinuteInput).toHaveDisplayValue('45');
    await user.clear(endTimeMinuteInput);
    await user.type(endTimeMinuteInput, '54');
    expect(endTimeMinuteInput).toHaveDisplayValue('54');
  });

  it('permits capacity data entry', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          session: {
            startTime: {
              hour: 9,
              minute: 30,
            },
            endTime: {
              hour: 17,
              minute: 45,
            },
            capacity: 1,
          },
        }}
      >
        <TimeAndCapacityStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    const capacityInput = screen.getByRole('textbox', {
      name: 'How many vaccinators or spaces do you have?',
    });

    expect(capacityInput).toHaveDisplayValue('1');
    await user.clear(capacityInput);
    await user.type(capacityInput, '5');
    expect(capacityInput).toHaveDisplayValue('5');
  });

  it('permits slot length data entry', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          session: {
            startTime: {
              hour: 9,
              minute: 30,
            },
            endTime: {
              hour: 17,
              minute: 45,
            },
            capacity: 1,
            slotLength: 7,
          },
        }}
      >
        <TimeAndCapacityStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    const capacityInput = screen.getByRole('textbox', {
      name: 'How long are your appointments?',
    });

    expect(capacityInput).toHaveDisplayValue('7');
    await user.clear(capacityInput);
    await user.type(capacityInput, '3');
    expect(capacityInput).toHaveDisplayValue('3');
  });
});
