import { parseToUkDatetime } from '@services/timeService';
import { DownloadReportFormValues } from './download-report-form-schema';
import BackLink from '@components/nhsuk-frontend/back-link';
import { Button, ButtonGroup } from '@components/nhsuk-frontend';

type DownloadReportConfirmationProps = {
  reportRequest: DownloadReportFormValues;
  goBack: () => void;
};

const DownloadReportConfirmation = ({
  reportRequest,
  goBack,
}: DownloadReportConfirmationProps) => {
  const requestFileDownload = () => {
    // TODO: Call the API which responds with a file download rather than JSON
    // Can we fire this straight to the client without having to implement anything ourselves?
    console.dir(reportRequest);
  };

  return (
    <>
      <BackLink renderingStrategy={'client'} onClick={goBack} text={'Back'} />
      <p>
        Download all data between
        {parseToUkDatetime(reportRequest.startDate).format('dddd, D MMMM YYYY')}
        and
        {parseToUkDatetime(reportRequest.endDate).format('dddd, D MMMM YYYY')}
      </p>
      <ButtonGroup>
        <Button
          type="button"
          styleType="secondary"
          action={requestFileDownload}
        >
          Create report
        </Button>
      </ButtonGroup>
    </>
  );
};

export default DownloadReportConfirmation;
