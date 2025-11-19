'use client';

import React from 'react';
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
  newlyUnsupportedBookingsCount: number;
  clinicalServices: ClinicalService[];
  session: string;
  newSession?: string | null;
  removedServicesSession?: string | null;
  site: string;
  date: string;
};

export const EditServicesConfirmationPage = ({
  newlyUnsupportedBookingsCount,
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
    formState: { errors, isSubmitted },
    getValues,
    setValue,
  } = useForm<FormData>({
    defaultValues: { action: undefined },
  });

  const recordDecision: SubmitHandler<FormData> = form => {
    setValue('action', form.action);
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
        `/site/${site}/availability/edit-services/confirmed?removedServicesSession=${removedServicesSession}&session=${session}&date=${date}&chosenAction=${form.action}&newlyUnsupportedBookingsCount=${response.bookingsCanceled}&cancelAppointments=${cancelBookings}&cancelledWithoutDetailsCount=${response.bookingsCanceledWithoutDetails}`,
      );
    });
  };

  const renderRadioForm = () => (
    <form onSubmit={handleSubmit(recordDecision)}>
      <FormGroup
        legend={
          newlyUnsupportedBookingsCount > 1
            ? 'Are you sure you want to cancel the appointments?'
            : 'Are you sure you want to cancel the appointment?'
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

  const renderConfirmationQuestion = (
    actionParam: SessionModificationAction | undefined,
  ) => (
    <form onSubmit={handleSubmit(submitForm)}>
      <h2>
        {newlyUnsupportedBookingsCount === 0 ||
        actionParam === 'remove-services'
          ? serviceCount > 1
            ? 'Are you sure you want to remove these services?'
            : 'Are you sure you want to remove this service?'
          : newlyUnsupportedBookingsCount > 1
            ? 'Are you sure you want to cancel the appointments?'
            : 'Are you sure you want to cancel the appointment?'}
      </h2>
      <ButtonGroup vertical>
        {pendingSubmit ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit" styleType="warning">
            {newlyUnsupportedBookingsCount === 0 ||
            actionParam === 'remove-services'
              ? serviceCount > 1
                ? 'Remove services'
                : 'Remove service'
              : newlyUnsupportedBookingsCount > 1
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
    if (!isSubmitted) return renderRadioForm();
    const action = getValues('action');
    return renderConfirmationQuestion(action);
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={[sessionSummaryDisplay]}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        showBooked={false}
      />

      {newlyUnsupportedBookingsCount > 0 ? (
        <>
          {isSubmitted && getValues('action') ? (
            <div className="margin-top-bottom">
              {getValues('action') === 'remove-services'
                ? newlyUnsupportedBookingsCount > 1
                  ? `You have chosen not to cancel ${newlyUnsupportedBookingsCount} bookings.`
                  : 'You have chosen not to cancel 1 booking.'
                : newlyUnsupportedBookingsCount > 1
                  ? `${newlyUnsupportedBookingsCount} bookings may have to be cancelled.`
                  : '1 booking may have to be cancelled.'}
            </div>
          ) : (
            <div className="margin-top-bottom">
              {`Removing ${
                serviceCount > 1 ? 'these services' : 'this service'
              } will affect ${newlyUnsupportedBookingsCount} ${
                newlyUnsupportedBookingsCount > 1 ? 'bookings' : 'booking'
              }.`}
            </div>
          )}

          {newlyUnsupportedBookingsCount &&
            getValues('action') != 'remove-services' && (
              <Card
                title={String(newlyUnsupportedBookingsCount)}
                description={
                  newlyUnsupportedBookingsCount > 1
                    ? 'Bookings may have to be cancelled'
                    : 'Booking may have to be cancelled'
                }
                maxWidth={250}
              />
            )}

          {isSubmitted && getValues('action') && (
            <div className="margin-top-bottom">
              {getValues('action') === 'cancel-appointments'
                ? 'People will be sent a text message or email confirming their appointment has been cancelled.'
                : ''}
            </div>
          )}

          {renderUnsupportedDecision()}
        </>
      ) : (
        renderConfirmationQuestion('remove-services')
      )}
    </>
  );
};
