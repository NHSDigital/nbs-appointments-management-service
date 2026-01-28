'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { useTransition } from 'react';
import { RFC3339Format } from '@services/timeService';
import SelectReportTypeStep from './select-report-type-step';
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
import * as yup from 'yup';

export enum ReportType {
  SiteSummary = 'site-summary',
  MasterSiteList = 'master-site-list',
  SitesUsers = 'site-users',
}

export type ReportsFormValues = DownloadReportFormValues & {
  reportType: ReportType;
};

interface Props {
  userPermissions: string[];
}

const ReportsTemplateWizard = ({ userPermissions }: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const today = ukNow();
  const reportsWizardSchema = downloadReportFormSchema.concat(
    yup.object({
      reportType: yup
        .mixed<ReportType>()
        .oneOf(Object.values(ReportType))
        .required(),
    }),
  );

  const methods = useForm<ReportsFormValues>({
    resolver: yupResolver(reportsWizardSchema),
    defaultValues: {
      startDate: today.format(RFC3339Format),
      endDate: today.format(RFC3339Format),
      reportType: undefined,
    },
  });
  const submitForm: SubmitHandler<ReportsFormValues> = async (
    form: ReportsFormValues,
  ) => {
    startTransition(async () => {
      let blobResponse;
      let fileName = '';

      switch (form.reportType) {
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
          initialStep={1}
          returnRouteUponCancellation={`/sites`}
          onCompleteFinalStep={() => {
            methods.handleSubmit(submitForm);
          }}
          pendingSubmit={pendingSubmit}
        >
          <WizardStep>
            {stepProps => (
              <SelectReportTypeStep
                {...stepProps}
                userPermissions={userPermissions}
              />
            )}
          </WizardStep>
          <WizardStep>
            {stepProps => <ReportDateRangeStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => <ReportConfirmationStep {...stepProps} />}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default ReportsTemplateWizard;
