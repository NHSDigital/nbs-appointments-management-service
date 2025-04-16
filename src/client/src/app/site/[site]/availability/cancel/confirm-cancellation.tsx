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
import { cancelSession } from '@services/appointmentsService';
import { ClinicalService, SessionSummary } from '@types';
import { useRouter } from 'next/navigation';
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
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<CancelSessionDecisionFormData>({});
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  const router = useRouter();
  const submitForm: SubmitHandler<CancelSessionDecisionFormData> = async (
    form: CancelSessionDecisionFormData,
  ) => {
    if (form.action === 'cancel-session') {
      await cancelSession(sessionSummary, site);
      router.push(`cancel/confirmed?session=${session}&date=${date}`);
    } else {
      router.push(
        `/site/${site}/view-availability/daily-appointments?date=${date}&page=1&tab=0`,
      );
    }
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
      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Continue</Button>
      )}
    </form>
  );
};

export default ConfirmCancellation;
