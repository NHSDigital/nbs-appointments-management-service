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
import {
  ClinicalService,
  SessionSummary,
  AvailabilitySession,
  Session,
} from '@types';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { editSession } from '@services/appointmentsService';
import { toTimeFormat } from '@services/timeService';
import fromServer from '@server/fromServer';

export type NewSessionFormValues = {
  newSession: AvailabilitySession;
  cancelOrphanedBookings?: boolean;
};

type Action = 'change-session' | 'cancel-appointments';

type UnsupportedAppointmentsFormData = {
  action?: Action;
  cancelOrphanedBookings?: boolean;
  existingSession: AvailabilitySession;
  newSession: AvailabilitySession;
};

type PageProps = {
  unsupportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  newSessionDetails: Session;
  sessionToEdit: Session;
  site: string;
  date: string;
};

export const EditSessionConfirmation = ({
  unsupportedBookingsCount,
  clinicalServices,
  session,
  newSessionDetails,
  sessionToEdit,
  site,
  date,
}: PageProps) => {
  const hasUnsupportedBookings = unsupportedBookingsCount > 0;
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const router = useRouter();

  const toAvailabilitySession = (_session: Session): AvailabilitySession => ({
    from: toTimeFormat(_session.startTime) ?? '',
    until: toTimeFormat(_session.endTime) ?? '',
    slotLength: _session.slotLength,
    capacity: _session.capacity,
    services: _session.services,
  });

  const {
    handleSubmit,
    register,
    formState: { errors },
  } = useForm<UnsupportedAppointmentsFormData>({
    defaultValues: {
      newSession: toAvailabilitySession(newSessionDetails),
      existingSession: toAvailabilitySession(sessionToEdit),
      cancelOrphanedBookings: false,
    },
  });

  const [unsupportedBookingsDecision, setUnsupportedBookingsDecision] =
    useState<Action>();

  const recordUnsupportedBookingsDecision: SubmitHandler<
    UnsupportedAppointmentsFormData
  > = async form => setUnsupportedBookingsDecision(form.action);

  const submitForm: SubmitHandler<UnsupportedAppointmentsFormData> = async (
    form,
    event,
  ) => {
    const action =
      (event?.target as HTMLButtonElement)?.dataset.action ?? 'change-session';

    const cancelAppointments = action === 'cancel-appointments';

    const newAvailabilitySession = form.newSession;

    const enrichedForm: UnsupportedAppointmentsFormData = {
      ...form,
      cancelOrphanedBookings: cancelAppointments,
      newSession: newAvailabilitySession,
      existingSession: form.existingSession,
    };

    await updateSession(enrichedForm, newAvailabilitySession);

    router.push(
      `/site/${site}/availability/edit/confirmed?updatedSession=${btoa(JSON.stringify(updateSession))}&date=${date}&canelAppointments=${cancelAppointments}`,
    );
  };

  const updateSession = async (
    form: UnsupportedAppointmentsFormData,
    updatedSession: AvailabilitySession,
  ) => {
    await fromServer(
      editSession({
        date,
        site: site,
        mode: 'Edit',
        sessions: [updatedSession],
        sessionToEdit: form.existingSession,
        cancelOrphanedBookings: form.cancelOrphanedBookings ?? false,
      }),
    );
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
          href={`/site/${site}/availability/edit?session=${editSession}&date=${date}`}
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
              You have chosen not to cancel {unsupportedBookingsCount} bookings.
            </div>
          ) : (
            <>
              <div className="margin-top-bottom">
                Changing the time and capacity will affect{' '}
                {unsupportedBookingsCount} bookings.
              </div>
              <Card
                title={unsupportedBookingsCount.toString()}
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
