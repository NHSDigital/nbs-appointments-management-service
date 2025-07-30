import render from '@testing/render';
import { screen } from '@testing-library/react';
import DownloadReportConfirmation from './download-report-confirmation';
import { DownloadReportFormValues } from './download-report-form-schema';

const mockOnBack = jest.fn();
const mockReportRequest: DownloadReportFormValues = {
  startDate: '2025-03-01',
  endDate: '2025-03-31',
};

describe('Download Report Confirmation', () => {
  it('renders', () => {
    render(
      <DownloadReportConfirmation
        reportRequest={mockReportRequest}
        goBack={mockOnBack}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Download the report' }),
    ).toBeInTheDocument();
  });

  it('confirms to the user the dates they are requesting', () => {
    render(
      <DownloadReportConfirmation
        reportRequest={mockReportRequest}
        goBack={mockOnBack}
      />,
    );

    expect(
      screen.getByText(
        'Download all data between Saturday, 1 March 2025 and Monday, 31 March 2025',
      ),
    ).toBeInTheDocument();
  });

  it('targets the file download route handler with the export button', () => {
    render(
      <DownloadReportConfirmation
        reportRequest={mockReportRequest}
        goBack={mockOnBack}
      />,
    );

    const exportButton = screen.getByRole('link', { name: 'Export data' });

    const expectedHref = `reports/download?startDate=${mockReportRequest.startDate}&endDate=${mockReportRequest.endDate}`;

    expect(exportButton).toHaveAttribute('href', expectedHref);
  });

  it('calls goBack when the back link is clicked', async () => {
    const { user } = render(
      <DownloadReportConfirmation
        reportRequest={mockReportRequest}
        goBack={mockOnBack}
      />,
    );

    await user.click(screen.getByRole('link', { name: 'Back' }));

    expect(mockOnBack).toHaveBeenCalled();
  });
});
