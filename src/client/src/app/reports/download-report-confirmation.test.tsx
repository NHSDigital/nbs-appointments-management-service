import render from '@testing/render';
import { screen, waitFor } from '@testing-library/react';
import DownloadReportConfirmation from './download-report-confirmation';
import { DownloadReportFormValues } from './download-report-form-schema';
import { saveAs } from 'file-saver';
import { downloadSiteSummaryReport } from '@services/appointmentsService';
import asServerActionResult from '@testing/asServerActionResult';

const mockOnBack = jest.fn();
const mockReportRequest: DownloadReportFormValues = {
  startDate: '2025-03-01',
  endDate: '2025-03-31',
};

jest.mock('file-saver', () => ({
  saveAs: jest.fn(),
}));

jest.mock('@services/appointmentsService');
const mockDownloadSiteSummaryReport = downloadSiteSummaryReport as jest.Mock;

describe('Download Report Confirmation', () => {
  beforeEach(() => {
    mockDownloadSiteSummaryReport.mockResolvedValue(
      asServerActionResult(new Blob(['csv content'], { type: 'text/csv' })),
    );
  });

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
        'Download all days between Saturday, 1 March 2025 and Monday, 31 March 2025.',
      ),
    ).toBeInTheDocument();
  });

  it('calls download and saveAs when export button is clicked', async () => {
    const { user } = render(
      <DownloadReportConfirmation
        reportRequest={mockReportRequest}
        goBack={mockOnBack}
      />,
    );

    const exportButton = screen.getByRole('button', {
      name: 'Download report',
    });
    await user.click(exportButton);

    await waitFor(() => {
      expect(downloadSiteSummaryReport).toHaveBeenCalledWith(
        mockReportRequest.startDate,
        mockReportRequest.endDate,
      );
      expect(saveAs).toHaveBeenCalledTimes(1);
    });
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
