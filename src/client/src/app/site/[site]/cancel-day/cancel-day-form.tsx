'use client';

import { useState } from 'react';
import {
  InsetText,
  FormGroup,
  RadioGroup,
  Radio,
  ButtonGroup,
  Button,
} from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { parseToUkDatetime } from '@services/timeService';
import { DaySummaryV2, ClinicalService } from '@types';

type Props = {
  date: string;
  siteId: string;
  daySummary: DaySummaryV2;
  clinicalServices: ClinicalService[];
};

export default function CancelDayForm({
  date,
  siteId,
  daySummary,
  clinicalServices,
}: Props) {
  const { replace } = useRouter();
  const parsedDate = parseToUkDatetime(date);

  // ✅ State to track yes/no radio
  const [cancelChoice, setCancelChoice] = useState<boolean | undefined>(
    undefined,
  );

  // ✅ State to track when user clicks "Continue"
  const [confirmStep, setConfirmStep] = useState(false);

  const handleContinue = () => {
    if (cancelChoice === true) {
      setConfirmStep(true); // show cancel day button
    } else {
      replace(`/site/${siteId}/view-availability/week?date=${date}`);
    }
  };

  const handleCancel = () => {
    // TODO: API call to cancel the day
    console.log('Day cancelled!');
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={daySummary.sessions}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        tableCaption={`Sessions for ${parsedDate.format('dddd D MMMM')}`}
      />
      <InsetText>
        {daySummary.bookedAppointments} booked appointments will be cancelled.
        We'll notify people that their appointment has been cancelled
      </InsetText>

      {!confirmStep ? (
        <form onSubmit={e => e.preventDefault()}>
          <FormGroup
            legend="Are you sure you want to cancel this day?"
            error=""
          >
            <RadioGroup>
              <Radio
                label="Yes, I want to cancel the appointments"
                hint="Cancel day"
                id="yes"
                value="true"
                checked={cancelChoice === true}
                onChange={() => setCancelChoice(true)}
              />
              <Radio
                label="No, I don't want to cancel the appointments"
                hint="I want to keep my day, do not cancel"
                id="no"
                value="false"
                checked={cancelChoice === false}
                onChange={() => setCancelChoice(false)}
              />
            </RadioGroup>
          </FormGroup>
          <ButtonGroup>
            <Button
              type="button"
              onClick={handleContinue}
              disabled={cancelChoice === undefined}
            >
              Continue
            </Button>
          </ButtonGroup>
        </form>
      ) : (
        <FormGroup legend="Are you sure you want to cancel this day?" error="">
          <>
            <ButtonGroup>
              <Button type="button" styleType="warning" onClick={handleCancel}>
                Cancel day
              </Button>
            </ButtonGroup>
            <Link
              href={`/site/${siteId}/view-availability/week?date=${date}`}
              className="nhsuk-link"
            >
              No, go back
            </Link>
          </>
        </FormGroup>
      )}
    </>
  );
}
