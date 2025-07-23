/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { Site } from '@types';
import { useState } from 'react';
import { DownloadReportFormValues } from './download-report-form-schema';
import DownloadReportForm from './download-report-form';
import DownloadReportConfirmation from './download-report-confirmation';

interface ReportsPageProps {
  site: Site;
}

export const ReportsPage = ({ site }: ReportsPageProps) => {
  // TODO: Atm this journey has 2 steps; one to select the dates and one to confirm the download.
  // For now this is implemented as a simple state toggle, but if a 3rd or more steps are added,
  // we should use the wizard pattern like the user mnanagement and availability creation pages
  const [reportRequest, setReportRequest] = useState<
    DownloadReportFormValues | undefined
  >(undefined);

  return (
    <>
      {reportRequest === undefined ? (
        <DownloadReportForm
          setReportRequest={setReportRequest}
          goBackHref={`/site/${site.id}`}
        />
      ) : (
        <DownloadReportConfirmation
          reportRequest={reportRequest}
          goBack={() => setReportRequest(undefined)}
        />
      )}
    </>
  );
};
