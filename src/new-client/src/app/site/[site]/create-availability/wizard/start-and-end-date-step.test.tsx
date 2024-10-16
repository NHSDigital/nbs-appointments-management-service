import render from '@testing/render';
import { screen } from '@testing-library/react';
import StartAndEndDateStep from './start-and-end-date-step';
import { AvailabilityPeriodFormValues } from './availability-period-wizard';
import MockForm from '@testing/mockForm';

const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockSetCurrentStep = jest.fn();

describe('Create Availability Page', () => {
  it('renders', async () => {
    render(
      <MockForm<AvailabilityPeriodFormValues> submitHandler={jest.fn()}>
        <StartAndEndDateStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    // TODO: Query this with designers for accessibility.
    // The caption and subheading are melded into one heading which screen readers will read out.
    // But this is straight from the design system guidelines, so...
    expect(
      screen.getByRole('heading', {
        name: 'Create availability period Add start and end dates for your availability period',
      }),
    ).toBeInTheDocument;
  });

  it('permits data entry', async () => {
    const { user } = render(
      <MockForm<AvailabilityPeriodFormValues> submitHandler={jest.fn()}>
        <StartAndEndDateStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    await user.type(screen.getAllByLabelText('Day')[0], '01');
    await user.type(screen.getAllByLabelText('Month')[0], '02');
    await user.type(screen.getAllByLabelText('Year')[0], '2041');

    expect(screen.getAllByLabelText('Day')[0]).toHaveDisplayValue('1');
    expect(screen.getAllByLabelText('Month')[0]).toHaveDisplayValue('2');
    expect(screen.getAllByLabelText('Year')[0]).toHaveDisplayValue('2041');

    await user.type(screen.getAllByLabelText('Day')[1], '7');
    await user.type(screen.getAllByLabelText('Month')[1], '08');
    await user.type(screen.getAllByLabelText('Year')[1], '2042');

    expect(screen.getAllByLabelText('Day')[1]).toHaveDisplayValue('7');
    expect(screen.getAllByLabelText('Month')[1]).toHaveDisplayValue('8');
    expect(screen.getAllByLabelText('Year')[1]).toHaveDisplayValue('2042');

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(mockGoToNextStep).toHaveBeenCalled();
  });

  it('Displays validation messages', async () => {
    const { user } = render(
      <MockForm<AvailabilityPeriodFormValues> submitHandler={jest.fn()}>
        <StartAndEndDateStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToPreviousStep={mockGoToPreviousStep}
        />
      </MockForm>,
    );

    await user.type(screen.getAllByLabelText('Day')[0], '01');
    await user.type(screen.getAllByLabelText('Month')[0], '02');
    await user.type(screen.getAllByLabelText('Year')[0], '2041');

    await user.type(screen.getAllByLabelText('Month')[1], '08');
    await user.type(screen.getAllByLabelText('Year')[1], '2042');

    await user.click(screen.getByRole('button', { name: 'Continue' }));

    expect(screen.getByText('Day is required')).toBeInTheDocument;
  });

  it.each([
    [undefined, '2', '2041', '7', '08', '2042', 'Day is required'],
    ['1', undefined, '2041', '7', '08', '2042', 'Month is required'],
    ['1', '2', undefined, '7', '08', '2042', 'Year is required'],
    ['1', '2', '2041', undefined, '08', '2042', 'Day is required'],
    ['1', '2', '2041', '7', undefined, '2042', 'Month is required'],
    ['1', '2', '2041', '7', '08', undefined, 'Year is required'],
    ['0', '2', '2041', '7', '08', '2042', 'Please enter a valid day'],
    ['1', '0', '2041', '7', '08', '2042', 'Please enter a valid month'],
    ['1', '2', '0', '7', '08', '2042', 'Please enter a valid year'],
    ['1', '2', '2041', '0', '08', '2042', 'Please enter a valid day'],
    ['1', '2', '2041', '7', '0', '2042', 'Please enter a valid month'],
    ['1', '2', '2041', '7', '08', '0', 'Please enter a valid year'],
  ])(
    'can validate date components: day %p, month %p, year %p should be: %p',
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
        <MockForm<AvailabilityPeriodFormValues> submitHandler={jest.fn()}>
          <StartAndEndDateStep
            stepNumber={1}
            currentStep={1}
            isActive
            setCurrentStep={mockSetCurrentStep}
            goToNextStep={mockGoToNextStep}
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

      expect(screen.getByText(expectedValidationMessage)).toBeInTheDocument;
    },
  );

  it('displays an href link when there are no previous wizard steps', async () => {
    render(
      <MockForm<AvailabilityPeriodFormValues> submitHandler={jest.fn()}>
        <StartAndEndDateStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
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
      <MockForm<AvailabilityPeriodFormValues> submitHandler={jest.fn()}>
        <StartAndEndDateStep
          stepNumber={2}
          currentStep={2}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
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