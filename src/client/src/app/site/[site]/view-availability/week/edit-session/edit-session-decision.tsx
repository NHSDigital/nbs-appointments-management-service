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
  clinicalServices: ClinicalService[];
};

type EditSessionDecisionFormData = {
  action?: 'edit-session' | 'cancel-session';
};

export const EditSessionDecision = ({
  site,
  sessionSummary,
  date,
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
    if (form.action === 'edit-session') {
      router.push(
        `/site/${site.id}/availability/edit?session=${sessionSummary}&date=${date}`,
      );
    } else {
      router.push(
        `/site/${site.id}/availability/cancel?session=${sessionSummary}&date=${date}`,
      );
    }
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
          You can only reduce time and/or capacity from this screen. If you want
          to increase availability for this day, you must create a new session.
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
