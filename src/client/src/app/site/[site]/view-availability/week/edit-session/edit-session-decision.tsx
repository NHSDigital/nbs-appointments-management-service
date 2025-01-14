'use client';
import {
  Button,
  ButtonGroup,
  FormGroup,
  Radio,
  RadioGroup,
} from '@components/nhsuk-frontend';
import { SessionSummary, Site } from '@types';
import { SubmitHandler, useForm } from 'react-hook-form';

type EditSessionDecisionProps = {
  site: Site;
  sessionSummary: SessionSummary;
};

type EditSessionDecisionFormData = {
  action: 'edit-session' | 'cancel-session';
};

export const EditSessionDecision = ({}: EditSessionDecisionProps) => {
  const methods = useForm<EditSessionDecisionFormData>({});

  const submitForm: SubmitHandler<EditSessionDecisionFormData> = async (
    form: EditSessionDecisionFormData,
  ) => {
    if (form.action === 'edit-session') {
      // Redirect to edit session page
    } else {
      // Redirect to cancel session page
    }
  };

  return (
    <form onSubmit={methods.handleSubmit(submitForm)}>
      <FormGroup legend="What do you want to do?">
        <RadioGroup>
          <Radio
            label="Change the length or capacity of this session"
            hint="Shorten the session length or remove capacity"
            id="edit-session"
            value="edit-session"
            {...methods.register('action')}
          />
          <Radio
            label="Cancel this session"
            hint="Cancel all booked appointments, and remove this session"
            id="cancel-session"
            value="cancel-session"
            {...methods.register('action')}
          />
        </RadioGroup>
      </FormGroup>
      <ButtonGroup>
        <Button type="submit">Continue</Button>
      </ButtonGroup>
    </form>
  );
};
