import render from '@testing/render';
import { screen } from '@testing-library/react';
import StartAndEndDateStep from './start-and-end-date-step';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import MockForm from '@testing/mockForm';
import { DefaultValues } from 'react-hook-form';
import {
  dateTimeStringFormat,
  DayJsType,
  ukNow,
  parseToUkDatetime,
  isDayAfterUkNow,
  isDayWithinUkYear,
} from '@services/timeService';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
    isDayAfterUkNow: jest.fn(),
    isDayWithinUkYear: jest.fn(),
  };
});
const mockUkNow = ukNow as jest.Mock<DayJsType>;
const mockIsDayAfterUkNow = isDayAfterUkNow as jest.Mock<boolean>;
const mockIsDayWithinUkYear = isDayWithinUkYear as jest.Mock<boolean>;

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultValues: DefaultValues<CreateAvailabilityFormValues> = {
  days: [],
  sessionType: 'repeating',
  session: {
    startTime: {
      hour: 9,
      minute: 0,
    },
    endTime: {
      hour: 17,
      minute: 0,
    },
    capacity: 1,
    slotLength: 5,
    services: [],
  },
};

describe('Start and End Date Step', () => {
  beforeEach(() => {
    jest.resetAllMocks();
    mockUkNow.mockReturnValue(
      parseToUkDatetime('2000-01-01T00:00:00', dateTimeStringFormat),
    );
  });

  afterEach(() => jest.resetAllMocks());

  it('renders', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <StartAndEndDateStep
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

    // TODO: Query this with designers for accessibility.
    // The caption and subheading are melded into one heading which screen readers will read out.
    // But this is straight from the design system guidelines, so...
    expect(
      screen.getByRole('heading', {
        name: 'Create weekly session Add start and end dates',
      }),
    ).toBeInTheDocument();
  });

  it('permits data entry', async () => {
    mockIsDayAfterUkNow.mockImplementation(arg => {
      if (arg === '2000-02-01' || arg === '2000-08-07') {
        return true;
      }
      return false;
    });

    mockIsDayWithinUkYear.mockReturnValue(true);

    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <StartAndEndDateStep
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

    await user.type(screen.getAllByLabelText('Day')[0], '01');
    await user.type(screen.getAllByLabelText('Month')[0], '02');
    await user.type(screen.getAllByLabelText('Year')[0], '2000');

    expect(screen.getAllByLabelText('Day')[0]).toHaveDisplayValue('1');
    expect(screen.getAllByLabelText('Month')[0]).toHaveDisplayValue('2');
    expect(screen.getAllByLabelText('Year')[0]).toHaveDisplayValue('2000');

    await user.type(screen.getAllByLabelText('Day')[1], '7');
    await user.type(screen.getAllByLabelText('Month')[1], '08');
    await user.type(screen.getAllByLabelText('Year')[1], '2000');

    expect(screen.getAllByLabelText('Day')[1]).toHaveDisplayValue('7');
    expect(screen.getAllByLabelText('Month')[1]).toHaveDisplayValue('8');
    expect(screen.getAllByLabelText('Year')[1]).toHaveDisplayValue('2000');

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockGoToNextStep).toHaveBeenCalled();
  });

  it('Displays validation messages', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <StartAndEndDateStep
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

    await user.type(screen.getAllByLabelText('Day')[0], '01');
    await user.type(screen.getAllByLabelText('Month')[0], '02');
    await user.type(screen.getAllByLabelText('Year')[0], '2000');

    await user.type(screen.getAllByLabelText('Month')[1], '08');
    await user.type(screen.getAllByLabelText('Year')[1], '2000');

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('Session end date must be a valid date'),
    ).toBeInTheDocument();
  });

  const startDateInvalidMessage = 'Session start date must be a valid date';
  const endDateInvalidMessage = 'Session end date must be a valid date';

  it.each([
    [undefined, '2', '2041', '7', '08', '2000', startDateInvalidMessage],
    ['1', undefined, '2041', '7', '08', '2000', startDateInvalidMessage],
    ['1', '2', undefined, '7', '08', '2000', startDateInvalidMessage],
    ['1', '2', '2041', undefined, '08', '2000', endDateInvalidMessage],
    ['1', '2', '2041', '7', undefined, '2000', endDateInvalidMessage],
    ['1', '2', '2041', '7', '08', undefined, endDateInvalidMessage],
    ['0', '2', '2041', '7', '08', '2000', startDateInvalidMessage],
    ['1', '0', '2041', '7', '08', '2000', startDateInvalidMessage],
    ['1', '2', '0', '7', '08', '2000', startDateInvalidMessage],
    ['1', '2', '2041', '0', '08', '2000', endDateInvalidMessage],
    ['1', '2', '2041', '7', '0', '2000', endDateInvalidMessage],
    ['1', '2', '2041', '7', '08', '0', endDateInvalidMessage],
  ])(
    'can validate date components: start date: day %p, month %p, year %p end date: day %p, month %p, year %p should be: %p',
    async (
      startDay: string | undefined,
      startMonth: string | undefined,
      startYear: string | undefined,
      endDay: string | undefined,
      endMonth: string | undefined,
      endYear: string | undefined,
      expectedValidationMessage: string,
    ) => {
      const { user } = render(
        <MockForm<CreateAvailabilityFormValues>
          submitHandler={jest.fn()}
          defaultValues={defaultValues}
        >
          <StartAndEndDateStep
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

      if (startDay) {
        await user.type(screen.getAllByLabelText('Day')[0], startDay);
      }
      if (startMonth) {
        await user.type(screen.getAllByLabelText('Month')[0], startMonth);
      }
      if (startYear) {
        await user.type(screen.getAllByLabelText('Year')[0], startYear);
      }
      if (endDay) {
        await user.type(screen.getAllByLabelText('Day')[1], endDay);
      }
      if (endMonth) {
        await user.type(screen.getAllByLabelText('Month')[1], endMonth);
      }
      if (endYear) {
        await user.type(screen.getAllByLabelText('Year')[1], endYear);
      }

      await user.click(screen.getByRole('button', { name: 'Continue' }));

      expect(screen.getByText(expectedValidationMessage)).toBeInTheDocument();
    },
  );

  it('does not permit start date to be set more than 1 year in the future', async () => {
    mockIsDayAfterUkNow.mockImplementation(arg =>
      arg === '2001-01-02' ? true : false,
    );

    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <StartAndEndDateStep
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

    await user.clear(screen.getAllByLabelText('Day')[0]);
    await user.type(screen.getAllByLabelText('Day')[0], '2');

    await user.clear(screen.getAllByLabelText('Month')[0]);
    await user.type(screen.getAllByLabelText('Month')[0], '1');

    await user.clear(screen.getAllByLabelText('Year')[0]);
    await user.type(screen.getAllByLabelText('Year')[0], '2001');

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('Session start date must be within the next year'),
    ).toBeInTheDocument();
  });

  it('does not permit end date to be set more than 1 year in the future', async () => {
    mockIsDayAfterUkNow.mockImplementation(arg =>
      arg === '2020-06-06' ? true : false,
    );

    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={{
          ...defaultValues,
          startDate: { day: 6, month: 6, year: 2000 },
        }}
      >
        <StartAndEndDateStep
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

    await user.clear(screen.getAllByLabelText('Day')[1]);
    await user.type(screen.getAllByLabelText('Day')[1], '10');
    await user.clear(screen.getAllByLabelText('Month')[1]);
    await user.type(screen.getAllByLabelText('Month')[1], '1');
    await user.clear(screen.getAllByLabelText('Year')[1]);
    await user.type(screen.getAllByLabelText('Year')[1], '2001');

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(
      screen.getByText('Session end date must be within the next year'),
    ).toBeInTheDocument();
  });

  it('displays an href link when there are no previous wizard steps', async () => {
    render(
      <MockForm<CreateAvailabilityFormValues> submitHandler={jest.fn()}>
        <StartAndEndDateStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation={'some-back-link'}
        />
      </MockForm>,
    );

    expect(screen.getByRole('link', { name: 'Go back' })).toHaveAttribute(
      'href',
      'some-back-link',
    );
  });

  it('displays a link which invokes GoToPreviousStep if there is a previous step', async () => {
    const { user } = render(
      <MockForm<CreateAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <StartAndEndDateStep
          stepNumber={2}
          currentStep={2}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    expect(screen.getByRole('link', { name: 'Go back' })).toHaveAttribute(
      'href',
      '',
    );

    await user.click(screen.getByRole('link', { name: 'Go back' }));
    expect(mockGoToPreviousStep).toHaveBeenCalled();
  });
});
