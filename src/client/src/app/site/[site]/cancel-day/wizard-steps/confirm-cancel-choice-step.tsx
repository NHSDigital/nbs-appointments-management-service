import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  ButtonGroup,
  FormGroup,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import Link from 'next/link';
import CancelDaySummary from '../cancel-day-summary';
import { DaySummaryV2, ClinicalService, Site } from '@types';

type ConfirmCancelChoiceStepProps = {
  site: Site;
  date: string;
  daySummary: DaySummaryV2;
  clinicalServices: ClinicalService[];
};

export const ConfirmCancelChoiceStep = ({
  goToPreviousStep,
  site,
  date,
  daySummary,
  clinicalServices,
}: InjectedWizardProps & ConfirmCancelChoiceStepProps) => {
  return (
    <>
      <BackLink
        onClick={goToPreviousStep}
        renderingStrategy="client"
        text="Back"
      />
      <NhsHeading title={`Cancel ${date}`} caption={site.name} />

      <CancelDaySummary
        clinicalServices={clinicalServices}
        date={date}
        daySummary={daySummary}
      />

      <FormGroup legend="Are you sure you want to cancel this day?" error="">
        <ButtonGroup>
          <Button type="submit" styleType="warning">
            Cancel day
          </Button>
        </ButtonGroup>
        <Link
          href={`/site/${site.id}/view-availability/week?date=${date}`}
          className="nhsuk-link"
        >
          No, go back
        </Link>
      </FormGroup>
    </>
  );
};
