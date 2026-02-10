'use client';
import { FormGroup, SmallSpinnerWithText } from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import { AVAILABILITY_EDIT_DRAFT_KEY } from '@constants';
import { ClinicalService, SessionSummary, Site } from '@types';
import { useRouter } from 'next/navigation';
import { Button, InsetText, Radios } from 'nhsuk-react-components';
import { useEffect, useTransition } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';

type EditSessionDecisionProps = {
  site: Site;
  sessionSummary: string;
  date: string;
  clinicalServices: ClinicalService[];
  cancelSessionUpliftedJourneyFlag: boolean;
};

type EditSessionDecisionFormData = {
  action?: 'edit-session' | 'edit-services' | 'cancel-session';
};

export const EditSessionDecision = ({
  site,
  sessionSummary,
  date,
  clinicalServices,
  cancelSessionUpliftedJourneyFlag,
}: EditSessionDecisionProps) => {
  const [pendingSubmit, startTransition] = useTransition();
  const router = useRouter();
  const {
    handleSubmit,
    register,
    formState: { errors },
  } = useForm<EditSessionDecisionFormData>({});

  useEffect(() => {
    const storedDraft = sessionStorage.getItem(AVAILABILITY_EDIT_DRAFT_KEY);
    if (storedDraft) {
      sessionStorage.removeItem(AVAILABILITY_EDIT_DRAFT_KEY);
    }
  }, []);

  const submitForm: SubmitHandler<EditSessionDecisionFormData> = async (
    form: EditSessionDecisionFormData,
  ) => {
    startTransition(async () => {
      let reroute = `/site/${site.id}/availability/`;
      switch (form.action) {
        case 'edit-session':
          reroute += `edit?session=${sessionSummary}&date=${date}`;
          break;
        case 'edit-services':
          reroute += `edit-services?session=${sessionSummary}&date=${date}`;
          break;
        case 'cancel-session':
          if (cancelSessionUpliftedJourneyFlag) {
            reroute += `cancel/confirmation?session=${sessionSummary}&date=${date}`;
          } else {
            reroute += `cancel?session=${sessionSummary}&date=${date}`;
          }
          break;
        default:
          throw new Error('Invalid form action');
      }

      router.push(reroute);
    });
  };

  const session: SessionSummary = JSON.parse(atob(sessionSummary));

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={[session]}
        clinicalServices={clinicalServices}
        showUnbooked={false}
      />

      {!cancelSessionUpliftedJourneyFlag && (
        <InsetText>
          <p>
            You can only reduce time, capacity or services from this screen. If
            you want to increase availability for this day, you must create a
            new session.
          </p>
        </InsetText>
      )}

      <form onSubmit={handleSubmit(submitForm)}>
        <FormGroup
          legend="What do you want to do?"
          error={errors.action?.message}
        >
          <Radios>
            <Radios.Item
              hint="Shorten session length or remove capacity"
              value="edit-session"
              id="edit-session"
              {...register('action', {
                required: { value: true, message: 'Select an option' },
              })}
            >
              Change the length or capacity of this session
            </Radios.Item>
            <Radios.Item
              value="cancel-session"
              id="cancel-session"
              {...register('action', {
                required: { value: true, message: 'Select an option' },
              })}
            >
              Cancel the session
            </Radios.Item>
            {Object.keys(session.totalSupportedAppointmentsByService).length >
              1 && (
              <Radios.Item
                hint="Remove one or more services from this session"
                value="edit-services"
                id="edit-services"
                {...register('action', {
                  required: { value: true, message: 'Select an option' },
                })}
              >
                Remove a service or multiple services
              </Radios.Item>
            )}
          </Radios>
        </FormGroup>
        {pendingSubmit ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit">Continue</Button>
        )}
      </form>
    </>
  );
};
