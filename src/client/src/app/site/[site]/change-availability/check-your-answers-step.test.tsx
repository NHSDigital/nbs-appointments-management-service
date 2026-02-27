import render from '@testing/render';
import { screen } from '@testing-library/react';
import CheckYourAnswersStep from './check-your-answers-step';
import MockForm from '@testing/mockForm';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';

const mockGoToPreviousStep = jest.fn();
const mockSetCurrentStep = jest.fn();

const defaultProps = {
  goToPreviousStep: mockGoToPreviousStep,
  setCurrentStep: mockSetCurrentStep,
  pendingSubmit: false,
  goToNextStep: jest.fn(),
  currentStep: 3,
  stepNumber: 3,
  isActive: true,
  goToLastStep: jest.fn(),
  returnRouteUponCancellation: 'cancellationRoute',
};

const renderComponent = (
  formValues: Partial<ChangeAvailabilityFormValues>,
  props = defaultProps,
) => {
  const mergedValues = {
    startDate: { day: '1', month: '10', year: '2026' },
    endDate: { day: '24', month: '12', year: '2026' },
    proposedCancellationSummary: { sessionCount: 5, bookingCount: 0 },
    ...formValues,
  } as ChangeAvailabilityFormValues;

  return render(
    <MockForm<ChangeAvailabilityFormValues>
      defaultValues={mergedValues}
      submitHandler={jest.fn()}
    >
      <CheckYourAnswersStep {...props} />
    </MockForm>,
  );
};

describe('CheckYourAnswersStep', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Date Formatting Logic', () => {
    it('formats dates within the same year correctly (omits first year)', () => {
      renderComponent({
        startDate: { day: '1', month: '10', year: '2026' },
        endDate: { day: '24', month: '12', year: '2026' },
      });

      expect(
        screen.getByText('1 October to 24 December 2026'),
      ).toBeInTheDocument();
    });

    it('formats dates across different years correctly (shows both years)', () => {
      renderComponent({
        startDate: { day: '15', month: '12', year: '2026' },
        endDate: { day: '15', month: '1', year: '2027' },
      });

      expect(
        screen.getByText('15 December 2026 to 15 January 2027'),
      ).toBeInTheDocument();
    });
  });

  describe('Summary List Rendering', () => {
    it('displays the correct number of sessions from form values', () => {
      renderComponent({
        proposedCancellationSummary: { sessionCount: 12, bookingCount: 2 },
      });

      expect(screen.getByText('Number of sessions')).toBeInTheDocument();
      expect(screen.getByText('12')).toBeInTheDocument();
    });

    it('calls setCurrentStep(2) when the "Change" link for Dates is clicked', async () => {
      const { user } = renderComponent({});

      const changeLink = screen.getByRole('button', { name: /Change/i });
      await user.click(changeLink);

      expect(mockSetCurrentStep).toHaveBeenCalledWith(2);
    });
  });

  describe('Actions and Submission', () => {
    it('calls goToPreviousStep when the Back link is clicked', async () => {
      const { user } = renderComponent({});

      const backLink = screen.getByText('Back');
      await user.click(backLink);

      expect(mockGoToPreviousStep).toHaveBeenCalledTimes(1);
    });

    it('renders the "Cancel sessions" button when not pending', () => {
      renderComponent({});

      const submitBtn = screen.getByRole('button', {
        name: /Cancel sessions/i,
      });
      expect(submitBtn).toBeInTheDocument();
      expect(screen.queryByText('Saving...')).not.toBeInTheDocument();
    });

    it('renders the spinner and disables interaction when pendingSubmit is true', () => {
      renderComponent({}, { ...defaultProps, pendingSubmit: true });

      expect(screen.getByText('Saving...')).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: /Cancel sessions/i }),
      ).not.toBeInTheDocument();
    });
  });
});
