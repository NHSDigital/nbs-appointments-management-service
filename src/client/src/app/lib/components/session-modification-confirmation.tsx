'use client';

import React, { useState } from 'react';
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
import { toTimeFormat } from '@services/timeService';
import { useRouter } from 'next/navigation';
import { editSession } from '@services/appointmentsService';
import fromServer from '@server/fromServer';

type Mode = 'edit' | 'cancel';
type FormData = {
  action?: Action;
  cancelUnsupportedBookings?: boolean;
  existingSession: AvailabilitySession;
  newSession: AvailabilitySession;
};
type Action =
  | 'change-session'
  | 'cancel-appointments'
  | 'keep-appointments'
  | 'cancel-session';

type Props = {
  unsupportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  newSessionDetails: Session;
  sessionToEdit: Session;
  site: string;
  date: string;
  mode?: Mode;
};

type RadioOption = {
  value: Action;
  label: string;
};

type ModeTextConfig = {
  legend: string;
  radioOptions: RadioOption[];
  confirmationQuestion: (action: Action) => string;
  confirmButtonText: (action: Action) => string;
  impactNote: (action: Action | undefined, count: number) => string;
  impactCard: (action: Action | undefined) => boolean;
  notificationNote: (action: Action | undefined) => string;
};

const MODE_TEXTS: Record<Mode, ModeTextConfig> = {
  edit: {
    legend: 'Do you want to change this session?',
    radioOptions: [
      {
        value: 'cancel-appointments',
        label: 'Yes, cancel the appointments and change this session',
      },
      {
        value: 'change-session',
        label: 'No, do not cancel the appointments but change this session',
      },
    ],
    confirmationQuestion: (action: Action) =>
      action === 'change-session'
        ? 'Are you sure you want to change this session?'
        : 'Are you sure you want to cancel the appointments?',
    confirmButtonText: (action: Action) =>
      action === 'change-session' ? 'Change session' : 'Cancel appointments',
    impactNote: (action: Action | undefined, count: number) =>
      action === 'change-session'
        ? `You have chosen not to cancel ${count} bookings.`
        : `Changing the time and capacity will affect ${count} bookings`,
    impactCard: (action: Action | undefined) =>
      action === undefined || action === 'cancel-appointments',
    notificationNote: (action: Action | undefined) =>
      action === 'change-session'
        ? ''
        : 'People will be sent a text message or email confirming their appointment has been cancelled.',
  },
  cancel: {
    legend: 'Do you want to cancel this session?',
    radioOptions: [
      {
        value: 'cancel-appointments',
        label: 'Yes, cancel the appointments and cancel the session',
      },
      {
        value: 'keep-appointments',
        label: 'No, do not cancel the appointments but cancel the session',
      },
    ],
    confirmationQuestion: (action: Action) =>
      action === 'keep-appointments'
        ? 'Are you sure you want to cancel the session?'
        : 'Are you sure you want to cancel the appointments?',
    confirmButtonText: (action: Action) =>
      action === 'keep-appointments' ? 'Cancel session' : 'Cancel appointments',
    impactNote: (action: Action | undefined, count: number) =>
      action === undefined
        ? `Some bookings will move to a different session on the same day and at the same time. Cancelling the session will affect ${count} bookings.`
        : action === 'keep-appointments'
          ? `You have chosen not to cancel ${count} bookings.`
          : `${count} bookings may have to be cancelled.`,
    impactCard: (action: Action | undefined) =>
      action === undefined || action === 'cancel-appointments',
    notificationNote: (action: Action | undefined) =>
      action === 'cancel-appointments'
        ? 'People will be sent a text message or email confirming their appointment has been cancelled.'
        : '',
  },
};

export const SessionModificationConfirmation = ({
  unsupportedBookingsCount,
  clinicalServices,
  session,
  newSessionDetails,
  sessionToEdit,
  site,
  date,
  mode = 'edit',
}: Props) => {
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
  } = useForm<FormData>({
    defaultValues: {
      newSession: toAvailabilitySession(newSessionDetails),
      existingSession: toAvailabilitySession(sessionToEdit),
      cancelUnsupportedBookings: false,
    },
  });

  const [decision, setDecision] = useState<Action | undefined>();
  const texts = MODE_TEXTS[mode];
  const recordDecision: SubmitHandler<FormData> = async form => {
    setDecision(form.action as Action);
  };

  const submitForm: SubmitHandler<FormData> = async (_, event) => {
    const action =
      (event?.target as HTMLButtonElement)?.dataset.action ?? 'change-session';

    if (action === 'change-session' || action === 'cancel-appointments') {
      // handle session edit or
      // handle session edit and cancel appointments
      const newAvailabilitySession = _.newSession;

      const cancelAppointments = action === 'cancel-appointments';

      const enrichedForm: FormData = {
        cancelUnsupportedBookings: cancelAppointments,
        newSession: toAvailabilitySession(newSessionDetails),
        existingSession: _.existingSession,
      };

      await updateSession(enrichedForm, newAvailabilitySession);

      router.push(
        `/site/${site}/availability/edit/confirmed?updatedSession=${btoa(JSON.stringify(newAvailabilitySession))}&date=${date}&chosenAction=${action}&unsupportedBookingsCount=${unsupportedBookingsCount}&cancelAppointments=${cancelAppointments}`,
      );
    } else if (action === 'cancel-session') {
      // handle session cancel
    } else {
      // handle session cancel and cancel appointments
    }
  };

  const updateSession = async (
    form: FormData,
    updatedSession: AvailabilitySession,
  ) => {
    await fromServer(
      editSession({
        date,
        site: site,
        mode: 'Edit',
        sessions: [updatedSession],
        sessionToEdit: form.existingSession,
        cancelUnsupportedBookings: form.cancelUnsupportedBookings ?? false,
      }),
    );
  };

  const renderRadioForm = () => (
    <form onSubmit={handleSubmit(recordDecision)}>
      <FormGroup legend={texts.legend} error={errors.action?.message}>
        <RadioGroup>
          {texts.radioOptions.map((opt: RadioOption) => (
            <Radio
              key={opt.value}
              label={opt.label}
              id={opt.value}
              value={opt.value}
              {...register('action', { required: 'Select an option' })}
            />
          ))}
        </RadioGroup>
      </FormGroup>

      <Button type="submit">Continue</Button>
    </form>
  );

  const renderConfirmationQuestion = (action: Action) => (
    <div>
      <h2>{texts.confirmationQuestion(action)}</h2>

      <ButtonGroup vertical>
        <Button
          type="button"
          styleType="warning"
          data-action={action}
          onClick={handleSubmit(submitForm)}
        >
          {texts.confirmButtonText(action)}
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
    if (!decision) return renderRadioForm();
    return renderConfirmationQuestion(decision);
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={[sessionSummary]}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        showBooked={false}
      />

      {unsupportedBookingsCount > 0 ? (
        <>
          {texts.impactNote && (
            <div className="margin-top-bottom">
              {texts.impactNote(decision, unsupportedBookingsCount)}
            </div>
          )}

          {texts.impactCard(decision) && (
            <Card
              title={String(unsupportedBookingsCount)}
              description="Bookings may have to be cancelled"
              maxWidth={250}
            />
          )}

          {texts.notificationNote(decision) && (
            <div className="margin-top-bottom">
              {texts.notificationNote(decision)}
            </div>
          )}

          {renderUnsupportedDecision()}
        </>
      ) : (
        renderConfirmationQuestion(
          mode === 'edit' ? 'change-session' : 'keep-appointments',
        )
      )}
    </>
  );
};
