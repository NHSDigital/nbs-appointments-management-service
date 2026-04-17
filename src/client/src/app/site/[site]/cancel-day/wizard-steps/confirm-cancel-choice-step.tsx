import { BackLink, ButtonGroup, FormGroup } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import Link from 'next/link';
import CancelDaySummary from '../cancel-day-summary';
import { DaySummaryV2, ClinicalService, Site } from '@types';
import { DayJsType, RFC3339Format } from '@services/timeService';
import { Heading, Button } from 'nhsuk-react-components';

export type ConfirmCancelChoiceStepProps = {
  site: Site;
  date: DayJsType;
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
      <Heading headingLevel="h2">
        <span className="nhsuk-caption-l">{site.name}</span>
        {`Cancel ${date.format('dddd D MMMM')}`}
      </Heading>

      <CancelDaySummary
        clinicalServices={clinicalServices}
        date={date.format('dddd D MMMM')}
        daySummary={daySummary}
      />

      <FormGroup legend="Are you sure you want to cancel this day?" error="">
        <ButtonGroup vertical>
          <Button type="submit" warning>
            Cancel day
          </Button>
          <Link
            href={`/site/${site.id}/view-availability/week?date=${date.format(RFC3339Format)}`}
            className="nhsuk-link"
          >
            No, go back
          </Link>
        </ButtonGroup>
      </FormGroup>
    </>
  );
};
