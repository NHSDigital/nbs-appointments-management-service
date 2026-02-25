import render from '@testing/render';
import { screen, within, waitFor } from '@testing-library/react';
import SelectDatesStep from './select-dates-step';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';
import MockForm from '@testing/mockForm';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});

const mockUkNow = ukNow as jest.Mock<DayJsType>;
const mockGoToNextStep = jest.fn();
const mockGoToPreviousStep = jest.fn();
const mockGoToLastStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultValues: ChangeAvailabilityFormValues = {
  startDate: { day: '', month: '', year: '' },
  endDate: { day: '', month: '', year: '' },
};

describe('SelectDatesStep', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockUkNow.mockReturnValue(parseToUkDatetime('2026-02-23'));
  });

  it('renders the heading and date input groups', () => {
    render(
      <MockForm<ChangeAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <SelectDatesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation=""
        />
      </MockForm>,
    );

    expect(screen.getByText('Select dates to cancel')).toBeInTheDocument();

    expect(
      screen.getByRole('group', { name: /start date/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('group', { name: /end date/i }),
    ).toBeInTheDocument();
  });

  it('navigates to next step when valid dates are entered', async () => {
    const { user } = render(
      <MockForm<ChangeAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <SelectDatesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation=""
        />
      </MockForm>,
    );

    const startGroup = screen.getByRole('group', { name: /start date/i });
    await user.type(within(startGroup).getByLabelText('Day'), '25');
    await user.type(within(startGroup).getByLabelText('Month'), '02');
    await user.type(within(startGroup).getByLabelText('Year'), '2026');

    const endGroup = screen.getByRole('group', { name: /end date/i });
    await user.type(within(endGroup).getByLabelText('Day'), '26');
    await user.type(within(endGroup).getByLabelText('Month'), '02');
    await user.type(within(endGroup).getByLabelText('Year'), '2026');

    await user.click(screen.getByRole('button', { name: /continue/i }));

    await waitFor(() => {
      expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
    });
  });

  it('calls goToPreviousStep when Back link is clicked', async () => {
    const { user } = render(
      <MockForm<ChangeAvailabilityFormValues>
        submitHandler={jest.fn()}
        defaultValues={defaultValues}
      >
        <SelectDatesStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation=""
        />
      </MockForm>,
    );

    await user.click(screen.getByText('Back'));
    expect(mockGoToPreviousStep).toHaveBeenCalledTimes(1);
  });
});
