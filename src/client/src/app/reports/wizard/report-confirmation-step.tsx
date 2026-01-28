'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  ButtonGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useFormContext } from 'react-hook-form';
import { ReportsFormValues, ReportType } from './reports-template-wizard';
import Link from 'next/link';
import { parseToUkDatetime } from '@services/timeService';

const ReportConfirmationStep = ({
  setCurrentStep,
  returnRouteUponCancellation,
  goToPreviousStep,
  pendingSubmit,
}: InjectedWizardProps) => {
  const { watch } = useFormContext<ReportsFormValues>();
  const formData = watch();
  const isSiteSummary = formData.reportType === ReportType.SiteSummary;

  const handleBack = () => {
    isSiteSummary ? goToPreviousStep() : setCurrentStep(1);
  };

  return (
    <>
      <BackLink onClick={handleBack} renderingStrategy="client" text="Back" />
      <br />
      <NhsHeading title="Download the report" />

      {isSiteSummary ? (
        <>
          <p>
            Download all days between
            {' ' +
              parseToUkDatetime(formData.startDate).format(
                'dddd, D MMMM YYYY',
              ) +
              ' '}
            and
            {' ' +
              parseToUkDatetime(formData.endDate).format('dddd, D MMMM YYYY') +
              '.'}
          </p>
          <p>
            Bookings availability and cancellations made today will not be
            available in this report.
          </p>
        </>
      ) : (
        <p>The report will be downloaded to your device as a CSV file.</p>
      )}

      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <ButtonGroup>
          <Button styleType="primary" type="submit">
            Download report
          </Button>
          <Link href={returnRouteUponCancellation}>Return to sites list</Link>
        </ButtonGroup>
      )}
    </>
  );
};

export default ReportConfirmationStep;
