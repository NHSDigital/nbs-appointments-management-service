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
import { parseToUkDatetime, RFC3339Format } from '@services/timeService';
import { DaySummaryV2, ClinicalService, CancelDayRequest } from '@types';
import { cancelDay } from '@services/appointmentsService';

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

  const handleCancel = async () => {
    const payload: CancelDayRequest = {
      site: siteId,
      date: parsedDate.format(RFC3339Format),
    };

    await cancelDay(payload);
    // TODO: APPT-1179 - use the response to the above & link to new page
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
