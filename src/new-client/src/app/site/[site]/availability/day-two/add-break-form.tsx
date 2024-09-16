/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Button, TextInput, Card } from '@nhsuk-frontend-components';
import { AvailabilityBlock } from '@types';
import { parseDate } from '@services/timeService';

type Props = {
  saveBlock: (block: AvailabilityBlock, oldBlock?: AvailabilityBlock) => void;
  date: string;
};

type FormFields = {
  startTime: string;
  endTime: string;
};

const AddBreakForm = ({ saveBlock, date }: Props) => {
  const { register, handleSubmit } = useForm<FormFields>({
    defaultValues: { startTime: '09:00', endTime: '17:00' },
  });

  const submitForm: SubmitHandler<FormFields> = async form => {
    saveBlock({
      day: parseDate(date),
      start: form.startTime,
      end: form.endTime,
      appointmentLength: 5,
      sessionHolders: 1,
      services: [],
      isPreview: false,
      isBreak: true,
    });
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <Card>
        <div
          className="nhsuk-grid-row"
          style={{ marginLeft: 0, marginRight: 0 }}
        >
          <h4>Break Details</h4>
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
        </div>
        <Button
          type="submit"
          styleType="secondary"
          style={{ marginBottom: 16 }}
        >
          Add break
        </Button>
      </Card>
    </form>
  );
};

export default AddBreakForm;
