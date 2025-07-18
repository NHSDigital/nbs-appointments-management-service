import { parseToUkDatetime } from '@services/timeService';
import { DownloadReportFormValues } from './download-report-form-schema';
import BackLink from '@components/nhsuk-frontend/back-link';
import { Button, ButtonGroup } from '@components/nhsuk-frontend';
import NhsHeading from '@components/nhs-heading';
import { Site } from '@types';
import Link from 'next/link';

type DownloadReportConfirmationProps = {
  reportRequest: DownloadReportFormValues;
  goBack: () => void;
  site: Site;
};

const DownloadReportConfirmation = ({
  reportRequest,
  goBack,
  site,
}: DownloadReportConfirmationProps) => {
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
      <ButtonGroup>
        <Link
          href={{
            pathname: 'reports/download',
            query: {
              site: site.id,
              startDate: reportRequest.startDate,
              endDate: reportRequest.endDate,
            },
          }}
        >
          <Button styleType="secondary">Export data</Button>
        </Link>
      </ButtonGroup>
    </>
  );
};

export default DownloadReportConfirmation;
