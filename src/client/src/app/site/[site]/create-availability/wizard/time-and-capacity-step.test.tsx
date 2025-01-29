import render from '@testing/render';
import { screen, waitFor } from '@testing-library/react';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import TimeAndCapacityStep from './time-and-capacity-step';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
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
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Create weekly session Set time and capacity for your session',
      }),
    ).toBeInTheDocument();
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
          goToLastStep={mockGoToLastStep}
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
    expect(endTimeHourInput).toHaveDisplayValue('06');

    expect(endTimeMinuteInput).toHaveDisplayValue('45');
    await user.clear(endTimeMinuteInput);
    await user.type(endTimeMinuteInput, '54');
    expect(endTimeMinuteInput).toHaveDisplayValue('54');
  });

  it.each([
    [undefined, '30', '8', '30', 'Enter a valid start time'],
    ['9', undefined, '8', '30', 'Enter a valid start time'],
    ['9', '30', undefined, '30', 'Enter a valid end time'],
    ['9', '30', '8', undefined, 'Enter a valid end time'],
    ['9', '30', '9', '30', 'Session length must be more than 5 minutes'],
    ['9', '0', '9', '04', 'Session length must be more than 5 minutes'],
    ['9', '30', '8', '30', 'Session end time must be after the start time'],
    ['0', '0', '0', '0', 'Session length must be more than 5 minutes'],
  ])(
    'validates start and end time entry',
    async (
      startHour: string | undefined,
      startMinute: string | undefined,
      endHour: string | undefined,
      endMinute: string | undefined,
      expectedMessage: string,
    ) => {
      const { user } = render(
        <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
          <TimeAndCapacityStep
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

      await user.clear(startTimeHourInput);
      if (startHour) {
        await user.type(startTimeHourInput, startHour);
      }

      await user.clear(startTimeMinuteInput);
      if (startMinute) {
        await user.type(startTimeMinuteInput, startMinute);
      }

      await user.clear(endTimeHourInput);
      if (endHour) {
        await user.type(endTimeHourInput, endHour);
      }

      await user.clear(endTimeMinuteInput);
      if (endMinute) {
        await user.type(endTimeMinuteInput, endMinute);
      }

      await user.click(screen.getByRole('button', { name: 'Continue' }));
      expect(screen.getByText(expectedMessage)).toBeInTheDocument();
    },
  );

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
          },
        }}
      >
        <TimeAndCapacityStep
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

    const capacityInput = screen.getByRole('spinbutton', {
      name: 'How many vaccinators or vaccination spaces do you have?',
    });

    expect(capacityInput).toHaveDisplayValue('');
    await user.clear(capacityInput);
    await user.type(capacityInput, '5');
    expect(capacityInput).toHaveDisplayValue('5');
  });

  it('validates capacity entry', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <TimeAndCapacityStep
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

    const capacityInput = screen.getByRole('spinbutton', {
      name: 'How many vaccinators or vaccination spaces do you have?',
    });

    await user.clear(capacityInput);

    await user.click(screen.getByRole('button', { name: 'Continue' }));
    expect(screen.getByText('Capacity is required')).toBeInTheDocument();
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
          },
        }}
      >
        <TimeAndCapacityStep
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

    const slotLengthInput = screen.getByRole('spinbutton', {
      name: 'How long are your appointments?',
    });

    expect(slotLengthInput).toHaveDisplayValue('');
    await user.clear(slotLengthInput);
    await user.type(slotLengthInput, '3');
    expect(slotLengthInput).toHaveDisplayValue('3');
  });

  it('validates slot length entry', async () => {
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
              minute: 30,
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
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    const slotLengthInput = screen.getByRole('spinbutton', {
      name: 'How long are your appointments?',
    });

    await user.clear(slotLengthInput);

    await user.click(screen.getByRole('button', { name: 'Continue' }));
    expect(
      screen.getByText('Appointment length is required'),
    ).toBeInTheDocument();
  });

  it('validates slot length is shorter than session length', async () => {
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
              hour: 9,
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
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    const slotLengthInput = screen.getByRole('spinbutton', {
      name: 'How long are your appointments?',
    });

    await user.clear(slotLengthInput);
    await user.type(slotLengthInput, '17');

    await user.click(screen.getByRole('button', { name: 'Continue' }));
    expect(
      screen.getByText(
        'Appointment length must be shorter than session length',
      ),
    ).toBeInTheDocument();
  });

  it('hides the appointment length time validation if start and end times are invalid', async () => {
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
              hour: 8,
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
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    const slotLengthInput = screen.getByRole('spinbutton', {
      name: 'How long are your appointments?',
    });

    await user.clear(slotLengthInput);
    await user.type(slotLengthInput, '5');

    await user.click(screen.getByRole('button', { name: 'Continue' }));
    expect(
      screen.getByText('Session end time must be after the start time'),
    ).toBeInTheDocument();
    expect(screen.queryByText('Appointment length is required')).toBeNull();
  });

  it('can be supplied with an alternative implementation for going back a step', async () => {
    const goToPreviousStepOverride = jest.fn();

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
              hour: 8,
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
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          goToPreviousStepOverride={goToPreviousStepOverride}
        />
      </MockForm>,
    );

    await user.click(screen.getByRole('link', { name: 'Go back' }));

    waitFor(() => {
      expect(goToPreviousStepOverride).toHaveBeenCalled();
    });
  });
});
