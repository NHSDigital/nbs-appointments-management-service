import render from '@testing/render';
import { screen, waitFor } from '@testing-library/react';
import CheckYourAnswersStep from './check-your-answers-step';
import MockForm from '@testing/mockForm';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import { cancelDateRange } from '@services/appointmentsService';
import { CancelDateRangeResponse, ServerActionResult } from '@types';

const mockGoToPreviousStep = jest.fn();
const mockGoToNextStep = jest.fn();
const mockSetCurrentStep = jest.fn();

jest.mock('@services/appointmentsService', () => {
  const originalModule = jest.requireActual('@services/appointmentsService');
  return {
    ...originalModule,
    cancelDateRange: jest.fn(),
  };
});
const mockCancelDateRange = cancelDateRange as jest.Mock<
  Promise<ServerActionResult<CancelDateRangeResponse>>
>;
const defaultProps = {
  goToPreviousStep: mockGoToPreviousStep,
  setCurrentStep: mockSetCurrentStep,
  pendingSubmit: false,
  goToNextStep: mockGoToNextStep,
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
      <CheckYourAnswersStep {...props} site="siteId" />
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

      const changeLink = screen.getByRole('link', { name: /Change Dates/i });
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

    it('does NOT navigate to next step if API returns failure', async () => {
      mockCancelDateRange.mockResolvedValue({ success: false });

      const { user } = renderComponent({});

      await user.click(
        screen.getByRole('button', { name: /Cancel sessions/i }),
      );

      await waitFor(() => {
        expect(mockCancelDateRange).toHaveBeenCalled();
        expect(mockGoToNextStep).not.toHaveBeenCalled();
      });
    });
    it('calls proposeCancelDateRange and navigates to next step on success', async () => {
      mockCancelDateRange.mockResolvedValue({
        success: true,
        data: {
          cancelledSessionsCount: 5,
          cancelledBookingsCount: 2,
          bookingsWithoutContactDetailsCount: 0,
        },
      });

      const { user } = renderComponent({
        startDate: { day: '1', month: '10', year: '2026' },
        endDate: { day: '24', month: '12', year: '2026' },
      });

      await user.click(
        screen.getByRole('button', { name: /Cancel sessions/i }),
      );

      await waitFor(() => {
        expect(mockCancelDateRange).toHaveBeenCalledWith(
          expect.objectContaining({
            from: expect.stringContaining('2026-10-01'),
            to: expect.stringContaining('2026-12-24'),
            cancelBookings: false,
          }),
        );

        expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
      });
    });
  });

  describe('Booking Cancellation Summary Row', () => {
    it('does not display the booking decision row if there are no bookings', () => {
      renderComponent({
        proposedCancellationSummary: { sessionCount: 5, bookingCount: 0 },
      });

      expect(
        screen.queryByText('What you have chosen to do with the bookings'),
      ).not.toBeInTheDocument();
    });

    it('displays "Keep bookings" when cancellationDecision is "keep-bookings"', () => {
      renderComponent({
        proposedCancellationSummary: { sessionCount: 5, bookingCount: 3 },
        cancellationDecision: 'keep-bookings',
      });

      expect(
        screen.getByText('What you have chosen to do with the bookings', {
          selector: 'dt',
        }),
      ).toBeInTheDocument();
      expect(screen.getByText('Keep bookings')).toBeInTheDocument();
    });

    it('displays "Cancel X bookings" when cancellationDecision is not "keep-bookings"', () => {
      renderComponent({
        proposedCancellationSummary: { sessionCount: 5, bookingCount: 10 },
        cancellationDecision: 'cancel-bookings',
      });

      expect(screen.getByText('Cancel 10 bookings')).toBeInTheDocument();
    });

    it('handles singular booking grammar correctly', () => {
      renderComponent({
        proposedCancellationSummary: { sessionCount: 1, bookingCount: 1 },
        cancellationDecision: 'cancel-bookings',
      });

      expect(screen.getByText('Cancel 1 booking')).toBeInTheDocument();
    });

    it('calls goToPreviousStep when the "Change" link for bookings is clicked', async () => {
      const { user } = renderComponent({
        proposedCancellationSummary: { sessionCount: 5, bookingCount: 5 },
        cancellationDecision: 'keep-bookings',
      });

      const changeLink = screen.getByRole('link', {
        name: /Change What you have chosen to do with the bookings/i,
      });

      await user.click(changeLink);

      expect(mockGoToPreviousStep).toHaveBeenCalledTimes(1);
    });
  });
});
