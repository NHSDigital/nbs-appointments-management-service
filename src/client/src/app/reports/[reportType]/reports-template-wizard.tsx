'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { useTransition } from 'react';
import { RFC3339Format } from '@services/timeService';
import ReportDateRangeStep from './report-date-range-step';
import ReportConfirmationStep from './report-confirmation-step';
import {
  downloadSiteSummaryReport,
  downloadMasterSiteListReport,
  downloadSiteUsersReport,
} from '@services/appointmentsService';
import fromServer from '@server/fromServer';
import { saveAs } from 'file-saver';
import { ukNow } from '@services/timeService';
import {
  DownloadReportFormValues,
  downloadReportFormSchema,
} from '../download-report-form-schema';
import { yupResolver } from '@hookform/resolvers/yup';

export enum ReportType {
  SiteSummary = 'site-summary',
  MasterSiteList = 'master-site-list',
  SitesUsers = 'site-users',
}

interface Props {
  reportType: ReportType;
}

const ReportsTemplateWizard = ({ reportType }: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const today = ukNow();
  const methods = useForm<DownloadReportFormValues>({
    resolver: yupResolver(downloadReportFormSchema),
    defaultValues: {
      startDate: today.format(RFC3339Format),
      endDate: today.format(RFC3339Format),
    },
  });
  const submitForm: SubmitHandler<DownloadReportFormValues> = async (
    form: DownloadReportFormValues,
  ) => {
    startTransition(async () => {
      let blobResponse;
      let fileName = '';

      switch (reportType) {
        case ReportType.SiteSummary:
          blobResponse = await fromServer(
            downloadSiteSummaryReport(form.startDate, form.endDate),
          );

          fileName = `GeneralSiteSummaryReport-${ukNow().format()}.csv`;
          break;

        case ReportType.MasterSiteList:
          blobResponse = await fromServer(downloadMasterSiteListReport());

          fileName = `MasterSiteListReport_-${ukNow().format()}.csv`;
          break;

        case ReportType.SitesUsers:
          blobResponse = await fromServer(downloadSiteUsersReport());

          fileName = `UserReport_Sites_${ukNow().format()}.csv`;
          break;

        default:
          return;
      }

      if (blobResponse) {
        saveAs(blobResponse, fileName);
      }
    });
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        <Wizard
          id="create-availability-wizard"
          initialStep={reportType == ReportType.SiteSummary ? 1 : 2}
          returnRouteUponCancellation={`/sites`}
          onCompleteFinalStep={() => {
            methods.handleSubmit(submitForm);
          }}
          pendingSubmit={pendingSubmit}
        >
          <WizardStep>
            {stepProps => <ReportDateRangeStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <ReportConfirmationStep {...stepProps} reportType={reportType} />
            )}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default ReportsTemplateWizard;
