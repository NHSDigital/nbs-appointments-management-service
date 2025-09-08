import { parseToUkDatetime } from '@services/timeService';
import { DownloadReportFormValues } from './download-report-form-schema';
import BackLink from '@components/nhsuk-frontend/back-link';
import { Button } from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import { ukNow } from '@services/timeService';
import { saveAs } from 'file-saver';
import { downloadSiteSummaryReport } from '@services/appointmentsService';

type DownloadReportConfirmationProps = {
  reportRequest: DownloadReportFormValues;
  goBack: () => void;
};

const DownloadReportConfirmation = ({
  reportRequest,
  goBack,
}: DownloadReportConfirmationProps) => {
  const handleDownload = async () => {
    const blobResponse = await downloadSiteSummaryReport(
      reportRequest.startDate,
      reportRequest.endDate,
    );

    saveAs(blobResponse, `GeneralSiteSummaryReport-${ukNow().format()}.csv`);
  };

  return (
    <>
      <BackLink renderingStrategy={'client'} onClick={goBack} text={'Back'} />
      <br />
      <NhsHeading title="Download the report" />
      <p>
        Download all data between{' '}
        {parseToUkDatetime(reportRequest.startDate).format('dddd, D MMMM YYYY')}{' '}
        and{' '}
        {parseToUkDatetime(reportRequest.endDate).format('dddd, D MMMM YYYY')}
      </p>
      <p>
        Bookings, availability, and cancellations made today will not be
        included in this report
      </p>
      <Button styleType="secondary" onClick={handleDownload}>
        Export data
      </Button>
    </>
  );
};

export default DownloadReportConfirmation;
