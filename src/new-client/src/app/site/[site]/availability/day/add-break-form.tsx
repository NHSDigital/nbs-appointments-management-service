/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React, { useEffect } from 'react';
import { SubmitHandler, useFormContext } from 'react-hook-form';
import { Button, TextInput, Card, FormGroup } from '@nhsuk-frontend-components';
import { AvailabilityBlock } from '@types';
import { minutesBetween, parseDate } from '@services/timeService';

type Props = {
  saveBlock: (block: AvailabilityBlock, oldBlockStart?: string) => void;
  date: string;
};

type FormFields = {
  startTime: string;
  endTime: string;
};

const AddBreakForm = ({ saveBlock, date }: Props) => {
  const { register, handleSubmit, watch, formState, trigger, reset } =
    useFormContext<FormFields>();

  const submitForm: SubmitHandler<FormFields> = async form => {
    saveBlock(
      {
        day: parseDate(date),
        start: form.startTime,
        end: form.endTime,
        appointmentLength: 5,
        sessionHolders: 1,
        services: [],
        isPreview: false,
        isBreak: true,
      },
      form.startTime,
    );

    reset();
  };

  const startTimeWatch = watch('startTime');
  const endTimeWatch = watch('endTime');

  useEffect(() => {
    trigger('startTime');
    trigger('endTime');
  }, [startTimeWatch, endTimeWatch, trigger]);

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <Card>
        <div
          className="nhsuk-grid-row"
          style={{ marginLeft: 0, marginRight: 0 }}
        >
          <FormGroup
            error={
              formState.errors.startTime?.message ||
              formState.errors.endTime?.message
            }
          >
            <h4>Break Details</h4>
            <TextInput
              label="Start time"
              id={'startTime'}
              {...register('startTime', {
                validate: (startTime: string) => {
                  const timeDiff = minutesBetween(startTime, endTimeWatch);
                  return timeDiff > 0 || 'Start time must be before end time';
                },
              })}
              type="time"
              style={{ width: '100px' }}
            ></TextInput>
            <TextInput
              label="End time"
              {...register('endTime', {
                validate: (endTime: string) => {
                  const timeDiff = minutesBetween(startTimeWatch, endTime);
                  return timeDiff > 0 || 'End time must be before start time';
                },
              })}
              type="time"
              style={{ width: '100px' }}
            ></TextInput>
          </FormGroup>
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
