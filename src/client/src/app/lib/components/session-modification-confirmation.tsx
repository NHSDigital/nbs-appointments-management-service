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
  SessionModificationAction,
  AvailabilitySession,
} from '@types';
import { modifySession } from '@services/appointmentsService';
import { toTimeFormat } from '@services/timeService';
import fromServer from '@server/fromServer';
import { useTransition } from 'react';
import { useRouter } from 'next/navigation';

type Mode = 'edit' | 'cancel';
type FormData = { action?: SessionModificationAction };

type Props = {
  newlyUnsupportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  newSession?: SessionSummary;
  site: string;
  date: string;
  mode: Mode;
};

type RadioOption = {
  value: SessionModificationAction;
  label: string;
};

type ModeTextConfig = {
  legend: string;
  radioOptions: RadioOption[];
  confirmationQuestion: (action: SessionModificationAction) => string;
  confirmButtonText: (action: SessionModificationAction) => string;
  impactNote: (
    action: SessionModificationAction | undefined,
    count: number,
  ) => string;
  impactCard: (action: SessionModificationAction | undefined) => boolean;
  notificationNote: (action: SessionModificationAction | undefined) => string;
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
    confirmationQuestion: (action: SessionModificationAction) =>
      action === 'change-session'
        ? 'Are you sure you want to change this session?'
        : 'Are you sure you want to cancel the appointments?',
    confirmButtonText: (action: SessionModificationAction) =>
      action === 'change-session' ? 'Change session' : 'Cancel appointments',
    impactNote: (
      action: SessionModificationAction | undefined,
      count: number,
    ) =>
      action === 'change-session'
        ? `You have chosen not to cancel ${count} bookings.`
        : `Changing the time and capacity will affect ${count} bookings`,
    impactCard: (action: SessionModificationAction | undefined) =>
      action === undefined || action === 'cancel-appointments',
    notificationNote: (action: SessionModificationAction | undefined) =>
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
    confirmationQuestion: (action: SessionModificationAction) =>
      action === 'keep-appointments'
        ? 'Are you sure you want to cancel the session?'
        : 'Are you sure you want to cancel the appointments?',
    confirmButtonText: (action: SessionModificationAction) =>
      action === 'keep-appointments' ? 'Cancel session' : 'Cancel appointments',
    impactNote: (
      action: SessionModificationAction | undefined,
      count: number,
    ) =>
      action === undefined
        ? `Cancelling the session will affect ${count} bookings.`
        : action === 'keep-appointments'
          ? `You have chosen not to cancel ${count} bookings.`
          : `${count} bookings may have to be cancelled.`,
    impactCard: (action: SessionModificationAction | undefined) =>
      action === undefined || action === 'cancel-appointments',
    notificationNote: (action: SessionModificationAction | undefined) =>
      action === 'cancel-appointments'
        ? 'People will be sent a text message or email confirming their appointment has been cancelled.'
        : '',
  },
};

const toAvailabilitySession = (
  session: SessionSummary,
): AvailabilitySession => ({
  from: toTimeFormat(session.ukStartDatetime) ?? '',
  until: toTimeFormat(session.ukEndDatetime) ?? '',
  slotLength: session.slotLength,
  capacity: session.capacity,
  services: Object.keys(session.totalSupportedAppointmentsByService),
});

export const SessionModificationConfirmation = ({
  newlyUnsupportedBookingsCount,
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

  if (mode == 'edit' && !newSession) {
    throw new Error('Cannot be in edit mode without a new session');
  }

  const {
    handleSubmit,
    register,
    formState: { errors },
    setValue,
  } = useForm<FormData>();
  const [decision, setDecision] = useState<
    SessionModificationAction | undefined
  >();
  const texts = MODE_TEXTS[mode];
  const recordDecision: SubmitHandler<FormData> = async form => {
    setDecision(form.action as SessionModificationAction);
    setValue('action', form.action as SessionModificationAction);
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
        newlyUnsupportedBookingAction: cancelBookings ? 'Cancel' : 'Orphan',
      };

      let updatedSessionSummary: AvailabilitySession =
        {} as AvailabilitySession;

      if (mode === 'edit' && newSession) {
        updatedSessionSummary = toAvailabilitySession(newSession);

        request = {
          ...request,
          sessionReplacement: {
            from: toTimeFormat(newSession.ukStartDatetime) || '',
            until: toTimeFormat(newSession.ukEndDatetime) || '',
            services: Object.keys(
              newSession.totalSupportedAppointmentsByService,
            ),
            slotLength: newSession.slotLength,
            capacity: newSession?.capacity,
          },
        };
      }

      const response = await fromServer(modifySession(request));

      const encode = (obj: unknown) => btoa(JSON.stringify(obj));
      router.push(
        `/site/${site}/availability/${mode}/confirmed?session=${mode === 'edit' ? encode(updatedSessionSummary) : session}&date=${date}&chosenAction=${form.action}&newlyUnsupportedBookingsCount=${newlyUnsupportedBookingsCount}&cancelAppointments=${cancelBookings}&cancelledWithoutDetailsCount=${response.bookingsCanceledWithoutDetails}`,
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

  const renderConfirmationQuestion = (action: SessionModificationAction) => (
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

        {mode == 'edit' && (
          <Link
            href={`/site/${site}/availability/edit?session=${session}&date=${date}`}
          >
            No, go back
          </Link>
        )}

        {mode == 'cancel' && (
          <Link
            href={`/site/${site}/view-availability/week/edit-session?date=${date}&session=${session}`}
          >
            No, go back
          </Link>
        )}
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
        sessionSummaries={
          mode == 'edit' && newSession ? [newSession] : [sessionSummary]
        }
        clinicalServices={clinicalServices}
        showUnbooked={false}
        showBooked={false}
      />

      {newlyUnsupportedBookingsCount > 0 ? (
        <>
          {texts.impactNote && (
            <div className="margin-top-bottom">
              {texts.impactNote(decision, newlyUnsupportedBookingsCount)}
            </div>
          )}

          {texts.impactCard(decision) && (
            <Card
              title={String(newlyUnsupportedBookingsCount)}
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
