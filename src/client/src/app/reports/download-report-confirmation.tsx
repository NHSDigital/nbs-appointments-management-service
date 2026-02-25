import { parseToUkDatetime } from '@services/timeService';
import { DownloadReportFormValues } from './download-report-form-schema';
import BackLink from '@components/nhsuk-frontend/back-link';
import {
  Button,
  ButtonGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import { ukNow } from '@services/timeService';
import { saveAs } from 'file-saver';
import { downloadSiteSummaryReport } from '@services/appointmentsService';
import fromServer from '@server/fromServer';
import { useTransition } from 'react';
import Link from 'next/link';

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
      <NhsHeading title="Download the report" />
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
          <Button styleType="primary" onClick={handleDownload}>
            Download report
          </Button>
          <Link href="/sites">Return to sites list</Link>
        </ButtonGroup>
      )}
    </>
  );
};

export default DownloadReportConfirmation;
