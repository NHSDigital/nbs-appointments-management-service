'use client';
import {
  Button,
  FormGroup,
  InsetText,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import fromServer from '@server/fromServer';
import { cancelSession } from '@services/appointmentsService';
import { ClinicalService, SessionSummary } from '@types';
import { useRouter } from 'next/navigation';
import { useTransition } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';

type PageProps = {
  date: string;
  session: string;
  site: string;
  clinicalServices: ClinicalService[];
};

type CancelSessionDecisionFormData = {
  action?: 'cancel-session' | 'dont-cancel-session';
};

const ConfirmCancellation = ({
  date,
  session,
  site,
  clinicalServices,
}: PageProps) => {
  const [pendingSubmit, startTransition] = useTransition();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CancelSessionDecisionFormData>({});
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const router = useRouter();
  const submitForm: SubmitHandler<CancelSessionDecisionFormData> = async (
    form: CancelSessionDecisionFormData,
  ) => {
    startTransition(async () => {
      if (form.action === 'cancel-session') {
        await fromServer(cancelSession(sessionSummary, site));
        router.push(`cancel/confirmed?updatedSession=${session}&date=${date}`);
      } else {
        router.push(
          `/site/${site}/view-availability/week/edit-session?session=${session}&date=${date}`,
        );
      }
    });
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <SessionSummaryTable
        sessionSummaries={[sessionSummary]}
        clinicalServices={clinicalServices}
      />

      <InsetText>
        <p>You'll need to manually cancel any affected appointments.</p>
      </InsetText>

      <FormGroup
        legend="Would you like to cancel this session?"
        error={errors.action?.message}
      >
        <RadioGroup>
          <Radio
            label="Yes, I want to cancel this session"
            id="cancel-session"
            value="cancel-session"
            {...register('action', {
              required: { value: true, message: 'Select an option' },
            })}
          />
          <Radio
            label="No, I don't want to cancel this session"
            id="dont-cancel-session"
            value="dont-cancel-session"
            {...register('action', {
              required: { value: true, message: 'Select an option' },
            })}
          />
        </RadioGroup>
      </FormGroup>
      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Continue</Button>
      )}
    </form>
  );
};

export default ConfirmCancellation;
