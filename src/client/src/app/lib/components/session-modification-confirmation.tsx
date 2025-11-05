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
  SessionModificationAction,
  AvailabilitySession,
} from '@types';
import { modifySession } from '@services/appointmentsService';
import { toTimeFormat } from '@services/timeService';
import { t } from '../../lib/locale';
import fromServer from '@server/fromServer';
import { useTransition } from 'react';
import { useRouter } from 'next/navigation';

type Mode = 'edit' | 'cancel' | 'edit-services';
type FormData = { action?: SessionModificationAction };

type Props = {
  unsupportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  newSession?: string | null;
  removedServicesSession?: string | null;
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
  radioOptions: { value: SessionModificationAction; labelKey: string }[];
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
    legend: 'legend.edit',
    radioOptions: [
      {
        value: 'cancel-appointments',
        labelKey: 'radio.edit.cancel-appointments',
      },
      { value: 'change-session', labelKey: 'radio.edit.change-session' },
    ],
    confirmationQuestion: action =>
      action === 'change-session'
        ? 'confirm.change-session'
        : 'confirm.cancel-appointments',
    confirmButtonText: action =>
      action === 'change-session'
        ? 'button.change-session'
        : 'button.cancel-appointments',
    impactNote: action =>
      action === 'change-session' ? 'impact.notCancel' : 'impact.change',
    impactCard: action =>
      action === undefined || action === 'cancel-appointments',
    notificationNote: action =>
      action === 'change-session' ? '' : 'notification.cancelled',
  },

  cancel: {
    legend: 'legend.cancel',
    radioOptions: [
      {
        value: 'cancel-appointments',
        labelKey: 'radio.cancel.cancel-appointments',
      },
      {
        value: 'keep-appointments',
        labelKey: 'radio.cancel.keep-appointments',
      },
    ],
    confirmationQuestion: action =>
      action === 'keep-appointments'
        ? 'confirm.cancel-session'
        : 'confirm.cancel-appointments',
    confirmButtonText: action =>
      action === 'keep-appointments'
        ? 'button.cancel-session'
        : 'button.cancel-appointments',
    impactNote: action =>
      action === undefined
        ? 'impact.cancelSession'
        : action === 'keep-appointments'
          ? 'impact.notCancel'
          : 'impact.cancelGeneric',
    impactCard: action =>
      action === undefined || action === 'cancel-appointments',
    notificationNote: action =>
      action === 'cancel-appointments' ? 'notification.cancelled' : '',
  },

  'edit-services': {
    // return the plural key based on count
    legend: 'legend.edit-services',
    radioOptions: [
      {
        value: 'cancel-appointments',
        labelKey: 'radio.edit-services.cancel-appointments',
      },
      {
        value: 'change-session',
        labelKey: 'radio.edit-services.change-session',
      },
    ],
    confirmationQuestion: action =>
      action === 'change-session'
        ? 'confirm.remove-services'
        : 'confirm.cancel-appointments',
    confirmButtonText: action =>
      action === 'change-session'
        ? 'button.change-session'
        : 'button.cancel-appointments',
    impactNote: action =>
      action === 'change-session' ? 'impact.notCancel' : 'impact.change',
    impactCard: action =>
      action === undefined || action === 'cancel-appointments',
    notificationNote: action =>
      action === 'change-session' ? '' : 'notification.cancelled',
  },
};

const toAvailabilitySession = (session: Session): AvailabilitySession => ({
  from: toTimeFormat(session.startTime) ?? '',
  until: toTimeFormat(session.endTime) ?? '',
  slotLength: session.slotLength,
  capacity: session.capacity,
  services: session.services,
});

export const SessionModificationConfirmation = ({
  unsupportedBookingsCount,
  clinicalServices,
  session,
  newSession,
  removedServicesSession,
  site,
  date,
  mode = 'edit',
}: Props) => {
  const router = useRouter();
  const [pendingSubmit, startTransition] = useTransition();
  const sessionSummary: SessionSummary = JSON.parse(atob(session));

  const removeServicesSessionDetails: AvailabilitySession | null =
    removedServicesSession ? JSON.parse(atob(removedServicesSession)) : null;

  const sessionSummaryDisplay: SessionSummary = JSON.parse(atob(session));

  if (mode === 'edit-services' && removeServicesSessionDetails) {
    for (const key of Object.keys(
      sessionSummaryDisplay.totalSupportedAppointmentsByService,
    )) {
      if (!removeServicesSessionDetails.services.includes(key)) {
        delete sessionSummaryDisplay.totalSupportedAppointmentsByService[key];
      }
    }
  }

  const serviceCount = Object.keys(
    sessionSummaryDisplay.totalSupportedAppointmentsByService,
  ).length;

  const appointmentCount = Array.isArray(
    sessionSummary.totalSupportedAppointments,
  )
    ? sessionSummary.totalSupportedAppointments.length
    : 0;

  const resolved = React.useMemo(() => {
    const cfg = MODE_TEXTS[mode];

    const legend = t(cfg.legend, { count: serviceCount });

    const radioOptions = cfg.radioOptions.map(ro => ({
      value: ro.value,
      label: t(ro.labelKey, { count: serviceCount }),
    }));

    const confirmationQuestion = (action: SessionModificationAction) =>
      t(cfg.confirmationQuestion(action), {
        count:
          mode === 'edit-services' ? serviceCount : unsupportedBookingsCount,
      });

    const confirmButtonText = (action: SessionModificationAction) =>
      t(cfg.confirmButtonText(action), {
        count: unsupportedBookingsCount,
      });

    const impactNote = (
      action: SessionModificationAction | undefined,
      count: number,
    ) => t(cfg.impactNote(action, count) as unknown as string, { count });

    const impactCard = cfg.impactCard;

    const notificationNote = (
      action: SessionModificationAction | undefined,
    ) => {
      if (!action) return '';

      return t(cfg.notificationNote(action));
    };

    return {
      legend,
      radioOptions,
      confirmationQuestion,
      confirmButtonText,
      impactNote,
      impactCard,
      notificationNote,
    };
  }, [mode, serviceCount, unsupportedBookingsCount]);

  const {
    handleSubmit,
    register,
    formState: { errors },
    setValue,
  } = useForm<FormData>();
  const [decision, setDecision] = useState<
    SessionModificationAction | undefined
  >();

  const recordDecision: SubmitHandler<FormData> = async form => {
    setDecision(form.action as SessionModificationAction);
    setValue('action', form.action as SessionModificationAction);
  };

  const submitForm: SubmitHandler<FormData> = async form => {
    startTransition(async () => {
      const newSessionSummary: Session | null = newSession
        ? JSON.parse(atob(newSession))
        : null;

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

      let updatedSessionSummary: AvailabilitySession =
        {} as AvailabilitySession;

      if (mode === 'edit' && newSessionSummary) {
        updatedSessionSummary = toAvailabilitySession(newSessionSummary);

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

      if (mode === 'edit-services' && removeServicesSessionDetails) {
        removedServicesSession;
        request = {
          ...request,
          sessionReplacement: {
            from: removeServicesSessionDetails.from,
            until: removeServicesSessionDetails.until,
            services: Object.keys(
              sessionSummary.totalSupportedAppointmentsByService,
            ).filter(
              key => !removeServicesSessionDetails?.services.includes(key),
            ),
            slotLength: removeServicesSessionDetails.slotLength,
            capacity: removeServicesSessionDetails.capacity,
          },
        };
      }

      const response = await fromServer(modifySession(request));

      const encode = (obj: unknown) => btoa(JSON.stringify(obj));

      let reroute = `/site/${site}/availability/${mode}/confirmed?`;
      if (mode === 'edit-services') {
        reroute += `removedServicesSession=${removedServicesSession}&`;
      }

      reroute += `session=${mode === 'edit' || mode === 'edit-services' ? encode(updatedSessionSummary) : session}&date=${date}&chosenAction=${form.action}&unsupportedBookingsCount=${response.bookingsCanceled}&cancelAppointments=${cancelBookings}&cancelledWithoutDetailsCount=${response.bookingsCanceledWithoutDetails}`;

      router.push(reroute);
    });
  };

  const renderRadioForm = () => (
    <form onSubmit={handleSubmit(recordDecision)}>
      <FormGroup legend={resolved.legend} error={errors.action?.message}>
        <RadioGroup>
          {resolved.radioOptions.map((opt: RadioOption) => (
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
      <h2>{resolved.confirmationQuestion(action)}</h2>

      <ButtonGroup vertical>
        {pendingSubmit ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit" styleType="warning">
            {resolved.confirmButtonText(action)}
          </Button>
        )}

        <Link
          href={`/site/${site}/availability/${mode === 'edit-services' ? 'edit-services' : 'edit'}?session=${session}&date=${date}`}
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
        sessionSummaries={[sessionSummaryDisplay]}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        showBooked={false}
      />

      {unsupportedBookingsCount > 0 ? (
        <>
          {resolved.impactNote && (
            <div className="margin-top-bottom">
              {resolved.impactNote(decision, unsupportedBookingsCount)}
            </div>
          )}

          {resolved.impactCard(decision) && (
            <Card
              title={String(unsupportedBookingsCount)}
              description="Bookings may have to be cancelled"
              maxWidth={250}
            />
          )}

          {resolved.notificationNote(decision) && (
            <div className="margin-top-bottom">
              {resolved.notificationNote(decision)}
            </div>
          )}

          {renderUnsupportedDecision()}
        </>
      ) : (
        renderConfirmationQuestion(
          mode === 'edit' || mode === 'edit-services'
            ? 'change-session'
            : 'keep-appointments',
        )
      )}
    </>
  );
};
