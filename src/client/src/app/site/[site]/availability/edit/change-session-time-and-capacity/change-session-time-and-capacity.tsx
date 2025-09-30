'use client';
import { useState, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { editSession } from '@services/appointmentsService';
import { AvailabilitySession, Site } from '@types';
type Props = {
  site: Site;
  orphanedCount: number;
  bookingsCount: number;
  bookingReferences: string[];
  updatedSession: AvailabilitySession;
  originalSession: AvailabilitySession;
  date: string;
};

type CancelDecisionFormData = {
  action?: 'cancel-and-change' | 'keep-and-change';
  actionIntent?: 'cancel-and-update' | 'update-only';
};

import { ChangeSessionSummaryTable } from './change-session-summary-table';
import {
  Button,
  ButtonGroup,
  FormGroup,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { cancelAppointment } from '@services/appointmentsService';
import fromServer from '@server/fromServer';

export default function ChangeSessionTimeAndCapacityPage({
  site,
  date,
  orphanedCount,
  bookingReferences,
  updatedSession,
  originalSession,
}: Props) {
  const [pendingSubmit] = useTransition();
  const [confirmationStep, setConfirmationStep] = useState<
    'initial' | 'confirm'
  >('initial');

  const [formData, setFormData] = useState<CancelDecisionFormData | null>(null);

  const router = useRouter();
  const {
    handleSubmit,
    register,
    setValue,
    formState: { errors },
  } = useForm<CancelDecisionFormData>({});

  const handleStagedSubmit: SubmitHandler<CancelDecisionFormData> = form => {
    setFormData(form);
    setConfirmationStep('confirm');
  };

  const submitForm: SubmitHandler<CancelDecisionFormData> = async form => {
    const { actionIntent } = form;
    const cancellationReason = 'CancelledBySite';

    let reroute = `/site/${site.id}/availability/`;

    if (actionIntent === 'cancel-and-update') {
      await Promise.all(
        bookingReferences.map(ref =>
          fromServer(cancelAppointment(ref, site.id, cancellationReason)),
        ),
      );

      reroute += `edit/confirmed?updatedSession=${btoa(JSON.stringify(updatedSession))}&date=${date}`;
    } else {
      reroute += `edit/confirmed?updatedSession=${btoa(JSON.stringify(updatedSession))}&date=${date}`;
    }

    await editSession({
      date,
      site: site.id,
      mode: 'Edit',
      sessions: [updatedSession],
      sessionToEdit: originalSession,
    });

    router.push(reroute);
  };

  return (
    <>
      <ChangeSessionSummaryTable sessionSummary={updatedSession} />

      {confirmationStep === 'initial' && (
        <>
          <div>
            <p>
              Changing the time and capacity will affect {orphanedCount} booking
              {orphanedCount === 1 ? '' : 's'}.
            </p>
            <div className="orphaned-warning-box">
              <div className="count">{orphanedCount}</div>
              <div className="message">
                Booking
                {orphanedCount === 1 ? ' ' : 's '}
                may have to be cancelled
              </div>
            </div>

            <p>
              People will be sent a text message or email confirming their
              appointment has been cancelled.
            </p>
          </div>

          <form onSubmit={handleSubmit(handleStagedSubmit)}>
            <FormGroup
              legend="Do you want to change this session?"
              error={errors.action?.message}
            >
              <RadioGroup>
                <Radio
                  label="Yes, cancel the appointments and change this session"
                  id="cancel-and-change"
                  value="cancel-and-change"
                  {...register('action', {
                    required: { value: true, message: 'Select an option' },
                  })}
                />
                <Radio
                  label="No, do not cancel the appointments but change this session"
                  id="keep-and-change"
                  value="keep-and-change"
                  {...register('action', {
                    required: { value: true, message: 'Select an option' },
                  })}
                />
              </RadioGroup>
            </FormGroup>

            {pendingSubmit ? (
              <SmallSpinnerWithText text="Working..." />
            ) : (
              <ButtonGroup>
                <Button type="submit">Continue</Button>
              </ButtonGroup>
            )}
          </form>
        </>
      )}
      {confirmationStep === 'confirm' && (
        <form
          onSubmit={handleSubmit(submitForm)}
          className="confirmation-screen"
        >
          <div className="confirmation-screen">
            {formData?.action === 'cancel-and-change' && (
              <div>
                <p>
                  Changing the time and capacity will affect {orphanedCount}{' '}
                  booking
                  {orphanedCount === 1 ? '' : 's'}.
                </p>
                <div className="orphaned-warning-box">
                  <div className="count">{orphanedCount}</div>
                  <div className="message">
                    Booking
                    {orphanedCount === 1 ? ' ' : 's '}
                    may have to be cancelled
                  </div>
                </div>
                <p>
                  People will be sent a text message or email confirming their
                  appointment has been cancelled.
                </p>
              </div>
            )}

            {formData?.action === 'keep-and-change' && (
              <p>
                You have chosen not to cancel {orphanedCount} booking
                {orphanedCount === 1 ? '' : 's'}.
              </p>
            )}

            <div className="nhs-form-group">
              <p className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
                Are you sure you want to{' '}
                {formData?.action === 'cancel-and-change'
                  ? 'cancel the appointments?'
                  : 'change the session?'}
              </p>
              <div>
                <Button
                  type="submit"
                  styleType="warning"
                  onClick={() => {
                    if (formData?.action === 'cancel-and-change')
                      setValue('actionIntent', 'cancel-and-update');
                    else setValue('actionIntent', 'update-only');
                  }}
                >
                  {formData?.action === 'cancel-and-change'
                    ? 'Cancel the appointments'
                    : 'Change session'}
                </Button>
              </div>

              <div>
                <a
                  href="#"
                  className="nhsuk-link"
                  onClick={e => {
                    e.preventDefault();
                    setConfirmationStep('initial');
                  }}
                >
                  No, go back
                </a>
              </div>
            </div>
          </div>
        </form>
      )}
    </>
  );
}
