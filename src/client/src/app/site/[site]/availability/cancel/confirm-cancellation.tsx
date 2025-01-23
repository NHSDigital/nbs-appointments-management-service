'use client';
import {
  Button,
  ButtonGroup,
  FormGroup,
  InsetText,
  Radio,
  RadioGroup,
} from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import { cancelSession } from '@services/appointmentsService';
import { SessionSummary } from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';

type PageProps = {
  date: string;
  session: string;
  site: string;
};

type CancelSessionDecisionFormData = {
  action?: 'cancel-session' | 'dont-cancel-session';
};

const ConfirmCancellation = ({ date, session, site }: PageProps) => {
  const methods = useForm<CancelSessionDecisionFormData>({});
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
    <form onSubmit={methods.handleSubmit(submitForm)}>
      <SessionSummaryTable sessionSummaries={[sessionSummary]} />

      <InsetText>
        <p>You'll need to manually cancel any affected appointments.</p>
      </InsetText>

      <FormGroup
        legend="Would you like to cancel this session?"
        error={methods.formState.errors.action?.message}
      >
        <RadioGroup>
          <Radio
            label="Yes, I want to cancel this session"
            id="cancel-session"
            value="cancel-session"
            {...methods.register('action', {
              required: { value: true, message: 'Select an option' },
            })}
          />
          <Radio
            label="No, I don't want to cancel this session"
            id="dont-cancel-session"
            value="dont-cancel-session"
            {...methods.register('action', {
              required: { value: true, message: 'Select an option' },
            })}
          />
        </RadioGroup>
      </FormGroup>
      <ButtonGroup>
        <Button type="submit">Continue</Button>
      </ButtonGroup>
    </form>
  );
};

export default ConfirmCancellation;
