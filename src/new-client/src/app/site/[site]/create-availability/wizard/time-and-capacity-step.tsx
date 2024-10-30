/* eslint-disable react/jsx-props-no-spreading */
'use client';
import NhsHeading from '@components/nhs-heading';
import { Button, FormGroup, TextInput } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { useFormContext } from 'react-hook-form';
import CapacityCalculation from './capacity-calculation';

const TimeAndCapacityStep = ({ goToNextStep }: InjectedWizardProps) => {
  const { register, watch } = useFormContext<CreateAvailabilityFormValues>();

  const [startTimeWatch, endTimeWatch, slotLengthWatch, capacityWatch] = watch([
    'session.startTime',
    'session.endTime',
    'session.slotLength',
    'session.capacity',
  ]);

  const onContinue = async () => {
    goToNextStep();
  };

  return (
    <>
      <NhsHeading
        title="Set time and capacity for your session"
        caption="Create availability period"
      />

      <p>
        You can add multiple sessions to this availability period, to cover part
        time work or different service types.
      </p>

      {/* TODO: Create nhsuk-frontend components for these time controls */}
      <FormGroup legend="Session times" hint="For example, 14:30">
        <div className="nhsuk-label">Start time</div>
        <div className="nhsuk-time-input-custom">
          <div className="nhsuk-time-input-custom__item">
            <input
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.startTime.hour')}
            ></input>
          </div>
          <div className="nhsuk-time-input-custom__item">
            <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
              :
            </div>
          </div>

          <div className="nhsuk-time-input-custom__item">
            <input
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.startTime.minute')}
            ></input>
          </div>
        </div>

        <div className="nhsuk-label">End time</div>
        <div className="nhsuk-time-input-custom">
          <div className="nhsuk-time-input-custom__item">
            <input
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.endTime.hour')}
            ></input>
          </div>
          <div className="nhsuk-time-input-custom__item">
            <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
              :
            </div>
          </div>

          <div className="nhsuk-time-input-custom__item">
            <input
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.endTime.minute')}
            ></input>
          </div>
        </div>
      </FormGroup>

      {/* <br />
      <FormGroup legend="Add a break to this session?">
        <RadioGroup>
          <Radio
            label="Yes"
            {...register('session.break')}
            id="break-yes"
            value="yes"
          />
          <Radio
            label="No"
            {...register('session.break')}
            id="break-no"
            value="no"
          />
        </RadioGroup>
      </FormGroup> */}

      <br />
      <FormGroup
        legend="Capacity"
        hint="Enter your capacity to calculate appointment numbers for this session"
      >
        <FormGroup legend="How many vaccinators or spaces do you have?">
          <TextInput
            {...register('session.capacity')}
            id="capacity"
            inputMode="numeric"
            width={2}
          ></TextInput>
        </FormGroup>

        <FormGroup legend="How long are your appointments?">
          <TextInput
            {...register('session.slotLength')}
            id="capacity"
            inputMode="numeric"
            width={2}
            suffix="minutes"
          ></TextInput>
        </FormGroup>
      </FormGroup>

      <CapacityCalculation
        startTime={startTimeWatch}
        endTime={endTimeWatch}
        slotLength={slotLengthWatch}
        capacity={capacityWatch}
      />
      <Button
        type="button"
        onClick={async () => {
          await onContinue();
        }}
      >
        Continue
      </Button>
    </>
  );
};

export default TimeAndCapacityStep;
