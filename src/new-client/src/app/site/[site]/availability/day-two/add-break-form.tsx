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
};

const AddBreakForm = () => {
  const { replace } = useRouter();
  const { register, handleSubmit, formState } = useForm<FormFields>({
    defaultValues: { startTime: '09:00', endTime: '17:00' },
  });

  const submitForm: SubmitHandler<FormFields> = async form => {
    console.dir(form);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <h4>Session Details</h4>
      <TextInput
        label="Start time"
        {...register('startTime')}
        type="time"
        style={{ width: '100px' }}
      ></TextInput>
      <TextInput
        label="End time"
        {...register('endTime')}
        type="time"
        style={{ width: '100px' }}
      ></TextInput>

      <Button type="submit" styleType="secondary" style={{ marginBottom: 0 }}>
        Add break
      </Button>
    </form>
  );
};

export default AddBreakForm;
