import render from '@testing/render';
import { screen } from '@testing-library/react';
import DownloadReportForm from './download-report-form';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});
const mockUkNow = ukNow as jest.Mock<DayJsType>;
const mockSetReportRequest = jest.fn();

describe('Download Report Confirmation', () => {
  beforeEach(() => {
    mockUkNow.mockReturnValue(parseToUkDatetime('2025-10-01'));
  });

  it('renders', () => {
    render(
      <DownloadReportForm
        setReportRequest={mockSetReportRequest}
        goBackHref={'/some-mock-url'}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Select the dates and create a report',
      }),
    ).toBeInTheDocument();
  });

  it('permits data input and invokes the callback on submit', async () => {
    const { user } = render(
      <DownloadReportForm
        setReportRequest={mockSetReportRequest}
        goBackHref={'/some-mock-url'}
      />,
    );

    // TODO: Select two dates from the datepicker

    await user.click(screen.getByRole('button', { name: 'Create report' }));
    expect(mockSetReportRequest).toHaveBeenCalledWith({
      startDate: '2025-10-01',
      endDate: '2025-10-01',
    });
  });

  it('calls goBack when the back link is clicked', () => {
    render(
      <DownloadReportForm
        setReportRequest={mockSetReportRequest}
        goBackHref={'/some-mock-url'}
      />,
    );

    const backLink = screen.getByRole('link', { name: 'Back' });
    expect(backLink).toHaveAttribute('href', '/some-mock-url');
  });
});
