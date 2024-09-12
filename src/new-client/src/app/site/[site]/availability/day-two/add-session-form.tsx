/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Role, RoleAssignment } from '@types';
import { useRouter } from 'next/navigation';
import { saveUserRoleAssignments } from '@services/appointmentsService';
import {
  Button,
  FormGroup,
  CheckBoxes,
  CheckBox,
  ButtonGroup,
  TextInput,
} from '@nhsuk-frontend-components';

type FormFields = {
  startTime: string;
  endTime: string;
  maxSimultaneousAppointments: number;
  appointmentLength: number;
};

const AddSessionForm = () => {
  const { replace } = useRouter();
  const { register, handleSubmit, formState } = useForm<FormFields>({
    defaultValues: {
      startTime: '09:00',
      endTime: '17:00',
      maxSimultaneousAppointments: 1,
      appointmentLength: 5,
    },
  });

  const submitForm: SubmitHandler<FormFields> = async form => {
    console.dir(form);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <h4>Session Details</h4>

      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-one-half">
          <TextInput
            label="Start time"
            {...register('startTime')}
            type="time"
            style={{ width: '250px' }}
          ></TextInput>
          <TextInput
            label="End time"
            {...register('endTime')}
            type="time"
            style={{ width: '250px' }}
          ></TextInput>
          <TextInput
            label="Max simultaneous appointments"
            hint="This could be based on the number of vaccinators or spaces you have available"
            {...register('maxSimultaneousAppointments')}
            type="number"
            style={{ width: '250px' }}
          ></TextInput>
          <TextInput
            label="Appointment length (Minutes)"
            {...register('appointmentLength')}
            type="number"
            style={{ width: '250px' }}
          ></TextInput>
        </div>
        <div className="nhsuk-grid-column-one-half">
          <label className="nhsuk-label" htmlFor={'checkboxes'}>
            Services
          </label>
          <CheckBoxes>
            <CheckBox label="Select all" />
            <CheckBox label="Covid" />
            <CheckBox label="Flu" />
            <CheckBox label="Shingles" />
            <CheckBox label="Pneumonia" />
            <CheckBox label="RSV" />
          </CheckBoxes>
        </div>
      </div>

      <Button type="submit" styleType="secondary" style={{ marginBottom: 0 }}>
        Add session
      </Button>
    </form>
  );
};

export default AddSessionForm;
