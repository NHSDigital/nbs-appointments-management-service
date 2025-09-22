'use client';
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

type EditSessionDecisionFormData = {
  action?: 'edit-session' | 'edit-services' | 'cancel-session';
};

export const EditSessionConfirmation = () => {
  const affectedBookings = 45;
  const {
    handleSubmit,
    register,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<EditSessionDecisionFormData>({});

  const submitForm: SubmitHandler<EditSessionDecisionFormData> = async (
    form: EditSessionDecisionFormData,
  ) => {
    //   let reroute = `/site/${site.id}/availability/`;
    //   switch (form.action) {
    //     case 'edit-session':
    //       reroute += `edit?session=${sessionSummary}&date=${date}`;
    //       break;
    //     case 'edit-services':
    //       reroute += `edit-services?session=${sessionSummary}&date=${date}`;
    //       break;
    //     case 'cancel-session':
    //       reroute += `cancel?session=${sessionSummary}&date=${date}`;
    //       break;
    //     default:
    //       throw new Error('Invalid form action');
    //   }
    //   router.push(reroute);
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={[]}
        clinicalServices={[]}
        showUnbooked={false}
      />

      <div>
        Changing the time and capacity will affect {affectedBookings} bookings.
      </div>

      <div className="cards-wrapper">
        <Card
          title="30"
          description="Bookings will be rellocated to another session"
          maxWidth={250}
        />
        <Card
          title="29"
          description="Appointments may have to be cancelled"
          maxWidth={250}
        />
      </div>

      <span>
        People will be sent a text message or email confirming their appointment
        has been cancelled.
      </span>

      <form onSubmit={handleSubmit(submitForm)}>
        <FormGroup
          legend="Do you want to change this session?"
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
            {/* {Object.keys(session.totalSupportedAppointmentsByService).length >
              1 && (
              <Radio
                label="Remove services from this session"
                hint="Remove booked appointments for individual services"
                id="edit-services"
                value="edit-services"
                {...register('action', {
                  required: { value: true, message: 'Select an option' },
                })}
              />
            )} */}
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
