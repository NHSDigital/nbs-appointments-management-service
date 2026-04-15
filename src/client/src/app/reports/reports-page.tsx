/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { useState, useEffect } from 'react';
import { DownloadReportFormValues } from './download-report-form-schema';
import DownloadReportForm from './download-report-form';
import DownloadReportConfirmation from './download-report-confirmation';
import { useRouter, useSearchParams } from 'next/navigation';

export const ReportsPage = () => {
  // TODO: Atm this journey has 2 steps; one to select the dates and one to confirm the download.
  // For now this is implemented as a simple state toggle, but if a 3rd or more steps are added,
  // we should use the wizard pattern like the user mnanagement and availability creation pages
  const [reportRequest, setReportRequest] = useState<
    DownloadReportFormValues | undefined
  >(undefined);
  const [originUrl, setOriginUrl] = useState<string | undefined>(undefined);

  const router = useRouter();
  const searchParams = useSearchParams();

  // Capture the returnUrl if it exists
  useEffect(() => {
    const returnUrl = searchParams.get('returnUrl');
    const ref = document.referrer;
    const isInternal = ref && ref.includes(window.location.host);

    if (returnUrl && isInternal) {
      setOriginUrl(returnUrl);
    }
  }, [searchParams]);

  const handleBack = () => {
    if (originUrl) {
      router.push(originUrl);
    } else {
      router.push('/sites');
    }
  };

  return (
    <>
      {reportRequest === undefined ? (
        <DownloadReportForm
          setReportRequest={setReportRequest}
          backLink={{
            renderingStrategy: 'client',
            onClick: handleBack,
            text: 'Back',
          }}
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
