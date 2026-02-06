import render from '@testing/render';
import { screen } from '@testing-library/react';
import ReportDateRangeStep from './report-date-range-step';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';
import MockForm from '@testing/mockForm';
import { DownloadReportFormValues } from '../download-report-form-schema';

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

describe('ReportDateRangeStep', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockUkNow.mockReturnValue(parseToUkDatetime('2025-10-01'));
  });

  it('navigates to next step when "Create report" is clicked', async () => {
    const { user } = render(
      <MockForm<DownloadReportFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ startDate: '2025-10-01', endDate: '2025-10-01' }}
      >
        <ReportDateRangeStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    await user.clear(screen.getByLabelText('Start date'));
    await user.type(screen.getByLabelText('Start date'), '2025-08-03');

    await user.clear(screen.getByLabelText('End date'));
    await user.type(screen.getByLabelText('End date'), '2025-08-04');

    await user.click(screen.getByRole('button', { name: 'Create report' }));

    expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
  });

  it('renders the date range step correctly', async () => {
    render(
      <MockForm<DownloadReportFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ startDate: '2025-10-01', endDate: '2025-10-01' }}
      >
        <ReportDateRangeStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation="/"
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Select the dates and run a report',
      }),
    ).toBeInTheDocument();
  });
});
