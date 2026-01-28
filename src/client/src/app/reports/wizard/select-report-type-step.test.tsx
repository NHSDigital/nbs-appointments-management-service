import render from '@testing/render';
import { screen } from '@testing-library/react';
import SelectReportTypeStep from './select-report-type-step';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';
import { ReportsFormValues } from './reports-template-wizard';
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

  it('renders only the site booking and capacity report card when no extra permissions are provided', async () => {
    render(
      <MockForm<ReportsFormValues> submitHandler={jest.fn()}>
        <SelectReportTypeStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation="/"
          userPermissions={[]}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Select a report' }),
    ).toBeInTheDocument();

    const siteSummaryCard = screen.getByRole('link', {
      name: 'Site booking and capacity report',
    });
    expect(siteSummaryCard).toBeInTheDocument();

    expect(screen.queryByText('All sites report')).not.toBeInTheDocument();
    expect(screen.queryByText('Users report')).not.toBeInTheDocument();
  });

  it('renders all sites report only with valid permissions', async () => {
    render(
      <MockForm<ReportsFormValues> submitHandler={jest.fn()}>
        <SelectReportTypeStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation="/"
          userPermissions={['reports:master-site-list']}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Select a report' }),
    ).toBeInTheDocument();

    const allSitesCard = screen.getByRole('link', { name: 'All sites report' });
    expect(allSitesCard).toBeInTheDocument();
  });

  it('renders users report only with valid permissions', async () => {
    render(
      <MockForm<ReportsFormValues> submitHandler={jest.fn()}>
        <SelectReportTypeStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation="/"
          userPermissions={['reports:siteusers']}
        />
      </MockForm>,
    );

    expect(
      screen.getByRole('heading', { name: 'Select a report' }),
    ).toBeInTheDocument();

    const usersCard = screen.getByRole('link', { name: 'Users report' });
    expect(usersCard).toBeInTheDocument();
  });

  it('renders report selection navigation correctly', async () => {
    const { user } = render(
      <MockForm<ReportsFormValues> submitHandler={jest.fn()}>
        <SelectReportTypeStep
          stepNumber={1}
          currentStep={1}
          isActive
          setCurrentStep={mockSetCurrentStep}
          goToNextStep={mockGoToNextStep}
          goToLastStep={mockGoToLastStep}
          goToPreviousStep={mockGoToPreviousStep}
          returnRouteUponCancellation="/"
          userPermissions={[]}
        />
      </MockForm>,
    );

    const siteSummaryCard = screen.getByRole('link', {
      name: 'Site booking and capacity report',
    });
    await user.click(siteSummaryCard);

    expect(mockGoToNextStep).toHaveBeenCalledTimes(1);
  });

  it.each([{ reportName: 'All sites report' }, { reportName: 'Users report' }])(
    'renders report selection navigation correctly',
    async ({ reportName }) => {
      const { user } = render(
        <MockForm<ReportsFormValues> submitHandler={jest.fn()}>
          <SelectReportTypeStep
            stepNumber={1}
            currentStep={1}
            isActive
            setCurrentStep={mockSetCurrentStep}
            goToNextStep={mockGoToNextStep}
            goToLastStep={mockGoToLastStep}
            goToPreviousStep={mockGoToPreviousStep}
            returnRouteUponCancellation="/"
            userPermissions={['reports:siteusers', 'reports:master-site-list']}
          />
        </MockForm>,
      );

      const siteSummaryCard = screen.getByRole('link', { name: reportName });
      await user.click(siteSummaryCard);

      expect(mockGoToLastStep).toHaveBeenCalledTimes(1);
    },
  );
});
