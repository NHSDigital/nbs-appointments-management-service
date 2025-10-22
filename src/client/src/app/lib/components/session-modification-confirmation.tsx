'use client';

import React, { useState } from 'react';
import { SessionSummaryTable } from '@components/session-summary-table';
import {
  Button,
  ButtonGroup,
  FormGroup,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Card } from '@nhsuk-frontend-components';
import Link from 'next/link';
import {
  ClinicalService,
  SessionSummary,
  UpdateSessionRequest,
  Session,
} from '@types';
import { modifySession } from '@services/appointmentsService';
import { toTimeFormat } from '@services/timeService';
import fromServer from '@server/fromServer';
import { useTransition } from 'react';
import { useRouter } from 'next/navigation';

type Mode = 'edit' | 'cancel';
type FormData = { action?: Action };
type Action =
  | 'change-session'
  | 'cancel-appointments'
  | 'keep-appointments'
  | 'cancel-session';

type Props = {
  unsupportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  newSession?: string | null;
  site: string;
  date: string;
  mode: Mode;
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
  newSession,
  site,
  date,
  mode = 'edit',
}: Props) => {
  const router = useRouter();
  const [pendingSubmit, startTransition] = useTransition();
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const newSessionSummary: Session | null = newSession
    ? JSON.parse(atob(newSession))
    : null;
  const {
    handleSubmit,
    register,
    formState: { errors },
    setValue,
  } = useForm<FormData>();
  const [decision, setDecision] = useState<Action | undefined>();
  const texts = MODE_TEXTS[mode];
  const recordDecision: SubmitHandler<FormData> = async form => {
    setDecision(form.action as Action);
    setValue('action', form.action as Action);
  };

  const submitForm: SubmitHandler<FormData> = async form => {
    startTransition(async () => {
      const cancelBookings = form.action === 'cancel-appointments';
      let request: UpdateSessionRequest = {
        from: date,
        to: date,
        site: site,
        sessionMatcher: {
          from: toTimeFormat(sessionSummary.ukStartDatetime) || '',
          until: toTimeFormat(sessionSummary.ukEndDatetime) || '',
          services: Object.keys(
            sessionSummary.totalSupportedAppointmentsByService,
          ),
          slotLength: sessionSummary.slotLength,
          capacity: sessionSummary.capacity,
        },
        sessionReplacement: null,
        cancelUnsupportedBookings: cancelBookings,
      };

      if (mode === 'edit' && newSessionSummary) {
        request = {
          ...request,
          sessionReplacement: {
            from: `${newSessionSummary.startTime.hour}:${newSessionSummary.startTime.minute}`,
            until: `${newSessionSummary.endTime.hour}:${newSessionSummary.endTime.minute}`,
            services: newSessionSummary.services,
            slotLength: newSessionSummary.slotLength,
            capacity: newSessionSummary.capacity,
          },
        };
      }

      await fromServer(modifySession(request));

      router.push(
        `/site/${site}/availability/${mode}/confirmed?updatedSession=${newSession}&date=${date}&cancelAppointments=${cancelBookings}`,
      );
    });
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
    <form onSubmit={handleSubmit(submitForm)}>
      <h2>{texts.confirmationQuestion(action)}</h2>

      <ButtonGroup vertical>
        {pendingSubmit ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit" styleType="warning">
            {texts.confirmButtonText(action)}
          </Button>
        )}

        <Link
          href={`/site/${site}/availability/edit?session=${session}&date=${date}`}
        >
          No, go back
        </Link>
      </ButtonGroup>
    </form>
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
