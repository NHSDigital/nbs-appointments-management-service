import render from '@testing/render';
import { screen } from '@testing-library/react';
import ReportConfirmationStep from './report-confirmation-step';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';
import { ReportsFormValues, ReportType } from './reports-template-wizard';
import MockForm from '@testing/mockForm';

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

describe('SelectReportTypeStep', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockUkNow.mockReturnValue(parseToUkDatetime('2025-10-01'));
  });

  it.each([
    { type: ReportType.MasterSiteList },
    { type: ReportType.SitesUsers },
  ])('renders confirmation step correctly', async ({ type }) => {
    render(
      <MockForm<ReportsFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ reportType: type }}
      >
        <ReportConfirmationStep
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
      screen.getByRole('heading', { name: 'Download the report' }),
    ).toBeInTheDocument();
    await expect(
      screen.getByText(
        'The report will be downloaded to your device as a CSV file.',
      ),
    ).toBeVisible();
    expect(
      screen.getByRole('button', { name: 'Download report' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Return to sites list' }),
    ).toBeInTheDocument();
  });

  it('renders confirmation step correctly for site summary report', async () => {
    render(
      <MockForm<ReportsFormValues>
        submitHandler={jest.fn()}
        defaultValues={{ reportType: ReportType.SiteSummary }}
      >
        <ReportConfirmationStep
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
      screen.getByRole('heading', { name: 'Download the report' }),
    ).toBeInTheDocument();
    await expect(screen.getByText(/download all days between/i)).toBeVisible();
    await expect(
      screen.getByText(
        'Bookings availability and cancellations made today will not be available in this report.',
      ),
    ).toBeVisible();
    expect(
      screen.getByRole('button', { name: 'Download report' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Return to sites list' }),
    ).toBeInTheDocument();
  });
});
