import render from '@testing/render';
import { screen } from '@testing-library/react';
import DownloadReportForm from './download-report-form';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';
import { BackLinkProps } from '@components/nhsuk-frontend/back-link';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});
const mockUkNow = ukNow as jest.Mock<DayJsType>;
const mockSetReportRequest = jest.fn();

const mockGoBack = jest.fn();
const goBackLinkProps: BackLinkProps = {
  renderingStrategy: 'client',
  text: 'Back',
  onClick: mockGoBack,
};

describe('Download Report Confirmation', () => {
  beforeEach(() => {
    mockUkNow.mockReturnValue(parseToUkDatetime('2025-10-01'));
  });

  it('renders', () => {
    render(
      <DownloadReportForm
        setReportRequest={mockSetReportRequest}
        backLink={goBackLinkProps}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Select the dates and run a report',
      }),
    ).toBeInTheDocument();
  });

  it('permits data input and invokes the callback on submit', async () => {
    const { user } = render(
      <DownloadReportForm
        setReportRequest={mockSetReportRequest}
        backLink={goBackLinkProps}
      />,
    );

    await user.clear(screen.getByLabelText('Start date'));
    await user.type(screen.getByLabelText('Start date'), '2025-08-03');

    await user.clear(screen.getByLabelText('End date'));
    await user.type(screen.getByLabelText('End date'), '2025-08-13');

    await user.click(screen.getByRole('button', { name: 'Create report' }));
    expect(mockSetReportRequest).toHaveBeenCalledWith({
      startDate: '2025-08-03',
      endDate: '2025-08-13',
    });
  });

  it('calls goBack when the back link is clicked', async () => {
    const { user } = render(
      <DownloadReportForm
        setReportRequest={mockSetReportRequest}
        backLink={goBackLinkProps}
      />,
    );

    const backLink = screen.getByRole('link', { name: 'Back' });
    await user.click(backLink);
    expect(mockGoBack).toHaveBeenCalledTimes(1);
  });
});
