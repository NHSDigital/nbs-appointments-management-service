import render from '@testing/render';
import { screen } from '@testing-library/react';
import DownloadReportConfirmation from './download-report-confirmation';
import { DownloadReportFormValues } from './download-report-form-schema';
import { mockSite } from '@testing/data';

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
        site={mockSite}
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
        site={mockSite}
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
        site={mockSite}
      />,
    );

    const exportButton = screen.getByRole('link', { name: 'Export data' });

    const expectedHref = `reports/download?site=${mockSite.id}&startDate=${mockReportRequest.startDate}&endDate=${mockReportRequest.endDate}`;

    expect(exportButton).toHaveAttribute('href', expectedHref);
  });

  it('calls goBack when the back link is clicked', async () => {
    const { user } = render(
      <DownloadReportConfirmation
        reportRequest={mockReportRequest}
        goBack={mockOnBack}
        site={mockSite}
      />,
    );

    await user.click(screen.getByRole('link', { name: 'Back' }));

    expect(mockOnBack).toHaveBeenCalled();
  });
});
