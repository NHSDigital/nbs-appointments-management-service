'use client';
import { SessionSummaryTable } from '@components/session-summary-table';
import {
  Button,
  ButtonGroup,
  FormGroup,
  Radio,
  RadioGroup,
} from '@components/nhsuk-frontend';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Card } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { ClinicalService, SessionSummary } from '@types';
import { useState } from 'react';

type Action = 'change-session' | 'cancel-appointments';

type UnsupportedAppointmentsFormData = {
  action?: Action;
};

type PageProps = {
  unsuportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  site: string;
  date: string;
};

export const EditSessionConfirmation = ({
  unsuportedBookingsCount,
  clinicalServices,
  session,
  site,
  date,
}: PageProps) => {
  const hasUnsupportedBookings = unsuportedBookingsCount > 0;
  const sessionSummary: SessionSummary = JSON.parse(atob(session));

  const {
    handleSubmit,
    register,
    formState: { errors },
  } = useForm<UnsupportedAppointmentsFormData>();

  const [unsupportedBookingsDecision, setUnsupportedBookingsDecision] =
    useState<Action>();

  const recordUnsupportedBookingsDecision: SubmitHandler<
    UnsupportedAppointmentsFormData
  > = async form => setUnsupportedBookingsDecision(form.action);

  const submitForm: SubmitHandler<UnsupportedAppointmentsFormData> = async (
    _,
    event,
  ) => {
    const action =
      (event?.target as HTMLButtonElement)?.dataset.action ?? 'change-session';

    if (action === 'change-session') {
      // handle change session
    } else {
      // handle cancel appointments
    }
  };

  const renderUnsupportedBookingsDecisionForm = () => (
    <form onSubmit={handleSubmit(recordUnsupportedBookingsDecision)}>
      <FormGroup
        legend="Do you want to change this session?"
        error={errors.action?.message}
      >
        <RadioGroup>
          <Radio
            label="Yes, cancel the appointments and change this session"
            id="cancel-appointments"
            value="cancel-appointments"
            {...register('action', { required: 'Select an option' })}
          />
          <Radio
            label="No, do not cancel the appointments but change this session"
            id="change-session"
            value="change-session"
            {...register('action', { required: 'Select an option' })}
          />
        </RadioGroup>
      </FormGroup>
      <Button type="submit">Continue</Button>
    </form>
  );

  const renderConfirmationQuestion = (action: Action) => (
    <div>
      <h2>
        {action === 'change-session'
          ? 'Are you sure you want to change this session?'
          : 'Are you sure you want to cancel the appointments?'}
      </h2>
      <ButtonGroup vertical>
        <Button
          type="button"
          styleType="warning"
          data-action={action}
          onClick={handleSubmit(submitForm)}
        >
          {action === 'change-session'
            ? 'Change session'
            : 'Cancel appointments'}
        </Button>
        <Link
          href={`/site/${site}/availability/edit?session=${session}&date=${date}`}
        >
          No, go back
        </Link>
      </ButtonGroup>
    </div>
  );

  const renderUnsupportedDecision = () => {
    if (!unsupportedBookingsDecision)
      return renderUnsupportedBookingsDecisionForm();
    return renderConfirmationQuestion(unsupportedBookingsDecision);
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={[sessionSummary]}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        showBooked={false}
      />

      {hasUnsupportedBookings ? (
        <>
          {unsupportedBookingsDecision === 'change-session' ? (
            <div className="margin-top-bottom">
              You have chosen not to cancel {unsuportedBookingsCount} bookings.
            </div>
          ) : (
            <>
              <div className="margin-top-bottom">
                Changing the time and capacity will affect{' '}
                {unsuportedBookingsCount} bookings.
              </div>
              <Card
                title={unsuportedBookingsCount.toString()}
                description="Bookings may have to be cancelled"
                maxWidth={250}
              />
              <div className="margin-top-bottom">
                People will be sent a text message or email confirming their
                appointment has been cancelled.
              </div>
            </>
          )}
          {renderUnsupportedDecision()}
        </>
      ) : (
        renderConfirmationQuestion('change-session')
      )}
    </>
  );
};
