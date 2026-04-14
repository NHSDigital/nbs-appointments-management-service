import { parseToUkDatetime } from '@services/timeService';
import { DownloadReportFormValues } from './download-report-form-schema';
import BackLink from '@components/nhsuk-frontend/back-link';
import { ButtonGroup, SmallSpinnerWithText } from '@components/nhsuk-frontend';
import { ukNow } from '@services/timeService';
import { saveAs } from 'file-saver';
import { downloadSiteSummaryReport } from '@services/appointmentsService';
import fromServer from '@server/fromServer';
import { useTransition } from 'react';
import Link from 'next/link';
import { Heading, Button } from 'nhsuk-react-components';

type DownloadReportConfirmationProps = {
  reportRequest: DownloadReportFormValues;
  goBack: () => void;
};

const DownloadReportConfirmation = ({
  reportRequest,
  goBack,
}: DownloadReportConfirmationProps) => {
  const [pendingSubmit, startTransition] = useTransition();
  const handleDownload = async () => {
    startTransition(async () => {
      const blobResponse = await fromServer(
        downloadSiteSummaryReport(
          reportRequest.startDate,
          reportRequest.endDate,
        ),
      );

      saveAs(blobResponse, `GeneralSiteSummaryReport-${ukNow().format()}.csv`);
    });
  };

  return (
    <>
      <BackLink renderingStrategy={'client'} onClick={goBack} text={'Back'} />
      <br />
      <Heading headingLevel="h2">Download the report</Heading>
      <p>
        Download all days between
        {' ' +
          parseToUkDatetime(reportRequest.startDate).format(
            'dddd, D MMMM YYYY',
          ) +
          ' '}
        and
        {' ' +
          parseToUkDatetime(reportRequest.endDate).format('dddd, D MMMM YYYY') +
          '.'}
      </p>
      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <ButtonGroup>
          <Button onClick={handleDownload}>Download report</Button>
          <Link href="/sites">Return to sites list</Link>
        </ButtonGroup>
      )}
    </>
  );
};

export default DownloadReportConfirmation;
