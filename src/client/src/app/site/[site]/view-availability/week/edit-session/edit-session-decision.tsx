'use client';
import {
  Button,
  ButtonGroup,
  FormGroup,
  InsetText,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import { ClinicalService, SessionSummary, Site } from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';

type EditSessionDecisionProps = {
  site: Site;
  sessionSummary: string;
  date: string;
  multipleServicesEnabled: boolean;
  clinicalServices: ClinicalService[];
};

type EditSessionDecisionFormData = {
  action?: 'edit-session' | 'edit-services' | 'cancel-session';
};

export const EditSessionDecision = ({
  site,
  sessionSummary,
  date,
  multipleServicesEnabled,
  clinicalServices,
}: EditSessionDecisionProps) => {
  const router = useRouter();
  const {
    handleSubmit,
    register,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<EditSessionDecisionFormData>({});

  const submitForm: SubmitHandler<EditSessionDecisionFormData> = async (
    form: EditSessionDecisionFormData,
  ) => {
    let reroute = `/site/${site.id}/availability/`;
    switch (form.action) {
      case 'edit-session':
        reroute += `edit?session=${sessionSummary}&date=${date}`;
        break;
      case 'edit-services':
        reroute += `edit-services?session=${sessionSummary}&date=${date}`;
        break;
      case 'cancel-session':
        reroute += `cancel?session=${sessionSummary}&date=${date}`;
        break;

      default:
        throw new Error('Invalid form action');
    }

    router.push(reroute);
  };

  const session: SessionSummary = JSON.parse(atob(sessionSummary));

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={[session]}
        clinicalServices={clinicalServices}
      />
      <InsetText>
        <p>
          You can only reduce time, capacity or services from this screen. If
          you want to increase availability for this day, you must create a new
          session.
        </p>
      </InsetText>
      <form onSubmit={handleSubmit(submitForm)}>
        <FormGroup
          legend="What do you want to do?"
          error={errors.action?.message}
        >
          <RadioGroup>
            <Radio
              label="Change the length or capacity of this session"
              hint="Shorten the session length or remove capacity"
              id="edit-session"
              value="edit-session"
              {...register('action', {
                required: { value: true, message: 'Select an option' },
              })}
            />
            {multipleServicesEnabled &&
              Object.keys(session.bookings).length > 1 && (
                <Radio
                  label="Remove services from this session"
                  hint="Remove booked appointments for individual services"
                  id="edit-services"
                  value="edit-services"
                  {...register('action', {
                    required: { value: true, message: 'Select an option' },
                  })}
                />
              )}
            <Radio
              label="Cancel this session"
              hint="Cancel all booked appointments, and remove this session"
              id="cancel-session"
              value="cancel-session"
              {...register('action', {
                required: { value: true, message: 'Select an option' },
              })}
            />
          </RadioGroup>
        </FormGroup>
        {isSubmitting || isSubmitSuccessful ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <ButtonGroup>
            <Button type="submit">Continue</Button>
          </ButtonGroup>
        )}
      </form>
    </>
  );
};
