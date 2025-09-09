'use client';

import { useState } from 'react';
import {
  InsetText,
  FormGroup,
  RadioGroup,
  Radio,
  ButtonGroup,
  Button,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { parseToUkDatetime, RFC3339Format } from '@services/timeService';
import { DaySummaryV2, ClinicalService, CancelDayRequest } from '@types';
import { cancelDay } from '@services/appointmentsService';
import { useForm } from 'react-hook-form';

type Props = {
  date: string;
  siteId: string;
  daySummary: DaySummaryV2;
  clinicalServices: ClinicalService[];
};

type FormFields = {
  cancelChoice: 'true' | 'false';
};

const CancelDayForm = ({
  date,
  siteId,
  daySummary,
  clinicalServices,
}: Props) => {
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<FormFields>();

  const { replace } = useRouter();
  const parsedDate = parseToUkDatetime(date);

  // ✅ State to track when user clicks "Continue"
  const [confirmStep, setConfirmStep] = useState(false);

  const handleContinue = (data: FormFields) => {
    if (data.cancelChoice === 'true') {
      setConfirmStep(true);
    } else {
      replace(`/site/${siteId}/view-availability/week?date=${date}`);
    }
  };

  console.log('This will cause a linting error');
  const foo = 1;
  // this will cause a tsc error
  foo = 'hi';

  const handleCancel = async () => {
    const payload: CancelDayRequest = {
      site: siteId,
      date: parsedDate.format(RFC3339Format),
    };

    const response = await cancelDay(payload);
    replace(
      `/site/${siteId}/cancel-day/confirmed?date=${date}&cancelledBookingCount=${response.cancelledBookingCount}&bookingsWithoutContactDetails=${response.bookingsWithoutContactDetails}`,
    );
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={daySummary.sessionSummaries}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        tableCaption={`Sessions for ${parsedDate.format('dddd D MMMM')}`}
      />
      <InsetText>
        {daySummary.totalSupportedAppointments} booked appointments will be
        cancelled. We'll notify people that their appointment has been cancelled
      </InsetText>

      {!confirmStep ? (
        <form onSubmit={handleSubmit(handleContinue)}>
          <FormGroup
            legend="Are you sure you want to cancel this day?"
            error={errors.cancelChoice?.message}
          >
            <RadioGroup>
              <Radio
                label="Yes, I want to cancel the appointments"
                hint="Cancel day"
                id="yes-cancel"
                value="true"
                {...register('cancelChoice', {
                  required: { value: true, message: 'Select an option' },
                })}
              />
              <Radio
                label="No, I don't want to cancel the appointments"
                hint="I want to keep my day, do not cancel"
                id="no"
                value="false"
                {...register('cancelChoice', {
                  required: { value: true, message: 'Select an option' },
                })}
              />
            </RadioGroup>
          </FormGroup>

          {isSubmitting || isSubmitSuccessful ? (
            <SmallSpinnerWithText text="Working..." />
          ) : (
            <Button type="submit" styleType="primary">
              Continue
            </Button>
          )}
        </form>
      ) : (
        <form onSubmit={handleSubmit(handleCancel)}>
          <FormGroup
            legend="Are you sure you want to cancel this day?"
            error=""
          >
            <>
              <ButtonGroup>
                <Button type="submit" styleType="warning">
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
        </form>
      )}
    </>
  );
};

export default CancelDayForm;
