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

type FormData = { action?: SessionModificationAction };

type Props = {
  unsupportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  newSession?: string | null;
  removedServicesSession?: string | null;
  site: string;
  date: string;
};

export const EditServicesConfirmationPage = ({
  unsupportedBookingsCount,
  clinicalServices,
  session,
  removedServicesSession,
  site,
  date,
}: Props) => {
  const router = useRouter();
  const [pendingSubmit, startTransition] = useTransition();
  const sessionSummary: SessionSummary = JSON.parse(atob(session));

  const removeServicesSessionDetails: AvailabilitySession | null =
    removedServicesSession ? JSON.parse(atob(removedServicesSession)) : null;

  const sessionSummaryDisplay: SessionSummary = JSON.parse(atob(session));

  if (removeServicesSessionDetails) {
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

      if (removeServicesSessionDetails) {
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

      router.push(
        `/site/${site}/availability/edit-services/confirmed?removedServicesSession=${removedServicesSession}&session=${session}&date=${date}&chosenAction=${form.action}&unsupportedBookingsCount=${response.bookingsCanceled}&cancelAppointments=${cancelBookings}&cancelledWithoutDetailsCount=${response.bookingsCanceledWithoutDetails}`,
      );
    });
  };

  const renderRadioForm = () => (
    <form onSubmit={handleSubmit(recordDecision)}>
      <FormGroup
        legend={
          serviceCount > 1
            ? 'Are you sure you want to remove these services?'
            : 'Are you sure you want to remove this service?'
        }
        error={errors.action?.message}
      >
        <RadioGroup>
          <Radio
            key="cancel-appointments"
            label={
              serviceCount > 1
                ? 'Yes, cancel the appointments and remove the services'
                : 'Yes, cancel the appointments and remove the service'
            }
            id="cancel-appointments"
            value="cancel-appointments"
            {...register('action', { required: 'Select an option' })}
          />
          <Radio
            key="remove-services"
            label={
              serviceCount > 1
                ? 'No, do not cancel the appointments but remove the services'
                : 'No, do not cancel the appointments but remove the service'
            }
            id="remove-services"
            value="remove-services"
            {...register('action', { required: 'Select an option' })}
          />
        </RadioGroup>
      </FormGroup>

      <Button type="submit">Continue</Button>
    </form>
  );

  const renderConfirmationQuestion = (action: SessionModificationAction) => (
    <form onSubmit={handleSubmit(submitForm)}>
      <h2>
        {unsupportedBookingsCount === 0 || action === 'remove-services'
          ? serviceCount > 1
            ? 'Are you sure you want to remove these services?'
            : 'Are you sure you want to remove this service?'
          : unsupportedBookingsCount > 1
            ? 'Are you sure you want to cancel the appointments?'
            : 'Are you sure you want to cancel the appointment?'}
      </h2>

      <ButtonGroup vertical>
        {pendingSubmit ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit" styleType="warning">
            {unsupportedBookingsCount === 0 || action === 'remove-services'
              ? serviceCount > 1
                ? 'Remove services'
                : 'Remove service'
              : unsupportedBookingsCount > 1
                ? 'Cancel appointments'
                : 'Cancel appointment'}
          </Button>
        )}

        <Link
          href={`/site/${site}/availability/edit-services?session=${session}&date=${date}`}
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
          {decision && (
            <div className="margin-top-bottom">
              {decision === 'remove-services'
                ? unsupportedBookingsCount > 1
                  ? `You have chosen not to cancel ${unsupportedBookingsCount} bookings.`
                  : 'You have chosen not to cancel 1 booking.'
                : unsupportedBookingsCount > 1
                  ? `${unsupportedBookingsCount} bookings may have to be cancelled.`
                  : '1 booking may have to be cancelled.'}
            </div>
          )}

          {!decision && (
            <Card
              title={String(unsupportedBookingsCount)}
              description={
                unsupportedBookingsCount > 1
                  ? 'Bookings may have to be cancelled'
                  : 'Booking may have to be cancelled'
              }
              maxWidth={250}
            />
          )}

          {decision && (
            <div className="margin-top-bottom">
              {decision === 'cancel-appointments'
                ? 'People will be sent a text message or email confirming their appointment has been cancelled.'
                : ''}
            </div>
          )}

          {renderUnsupportedDecision()}
        </>
      ) : (
        renderConfirmationQuestion('change-session')
      )}
    </>
  );
};
