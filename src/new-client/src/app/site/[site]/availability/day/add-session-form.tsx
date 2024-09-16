/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import { Controller, SubmitHandler, useFormContext } from 'react-hook-form';
import {
  Button,
  CheckBoxes,
  CheckBox,
  TextInput,
  Card,
  Select,
  FormGroup,
} from '@nhsuk-frontend-components';
import AppointmentsSummaryText from './appointments-summary-text';
import {
  covidServices,
  fluServices,
  shinglesServices,
  pneumoniaServices,
  rsvServices,
} from '@services/availabilityService';
import { hoursBetween, minutesBetween, parseDate } from '@services/timeService';
import { AvailabilityBlock } from '@types';
import { FormFields } from './day-page';

type Props = {
  saveBlock: (block: AvailabilityBlock, oldBlock?: AvailabilityBlock) => void;
  date: string;
};

const AddSessionForm = ({ saveBlock, date }: Props) => {
  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    formState,
    trigger,
    setError,
  } = useFormContext<FormFields>();

  const covidWatch = watch('services.covid');
  const covidAgesWatch = watch('services.covidAges');
  const fluWatch = watch('services.flu');
  const fluAgesWatch = watch('services.fluAges');
  const shinglesWatch = watch('services.shingles');
  const shinglesAgesWatch = watch('services.shinglesAges');
  const pneumoniaWatch = watch('services.pneumonia');
  const pneumoniaAgesWatch = watch('services.pneumoniaAges');
  const rsvWatch = watch('services.rsv');
  const rsvAgesWatch = watch('services.rsvAges');

  const appointmentLengthWatch = watch('appointmentLength');
  const maxSimultaneousAppointmentsWatch = watch('maxSimultaneousAppointments');
  const startTimeWatch = watch('startTime');
  const endTimeWatch = watch('endTime');

  const submitForm: SubmitHandler<FormFields> = async form => {
    if (
      !form.services.covid &&
      !form.services.flu &&
      !form.services.shingles &&
      !form.services.pneumonia &&
      !form.services.rsv
    ) {
      setError('services', { message: 'You must select at least one service' });
      return;
    }
    const collapsedServices = [
      ...form.services.covidAges,
      ...form.services.fluAges,
      ...form.services.pneumoniaAges,
      ...form.services.shinglesAges,
      ...form.services.rsvAges,
    ];
    saveBlock({
      day: parseDate(date),
      start: form.startTime,
      end: form.endTime,
      appointmentLength: form.appointmentLength,
      sessionHolders: form.maxSimultaneousAppointments,
      services: collapsedServices,
      isPreview: false,
      isBreak: false,
    });
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <Card>
        <div
          className="nhsuk-grid-row"
          style={{ marginLeft: 0, marginRight: 0 }}
        >
          <h4>Session Details</h4>
          <div className="nhsuk-grid-column-one-half">
            <FormGroup
              error={
                formState.errors.startTime?.message ||
                formState.errors.endTime?.message
              }
            >
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
                style={{ width: '250px' }}
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
                style={{ width: '250px' }}
              ></TextInput>
            </FormGroup>
            <Select
              label="Max simultaneous appointments"
              hint="This could be based on the number of vaccinators or spaces you have available"
              {...register('maxSimultaneousAppointments')}
              options={[
                { value: 1, label: '1' },
                { value: 2, label: '2' },
                { value: 3, label: '3' },
                { value: 4, label: '4' },
                { value: 5, label: '5' },
                { value: 6, label: '6' },
                { value: 7, label: '7' },
                { value: 8, label: '8' },
                { value: 9, label: '9' },
                { value: 10, label: '10' },
              ]}
              style={{ width: '250px' }}
            ></Select>
            <Select
              label="Appointment length (Minutes)"
              {...register('appointmentLength')}
              options={[
                { value: 4, label: '4' },
                { value: 5, label: '5' },
                { value: 6, label: '6' },
                { value: 10, label: '10' },
                { value: 12, label: '12' },
                { value: 15, label: '15' },
              ]}
              style={{ width: '250px' }}
            ></Select>
          </div>
          <div className="nhsuk-grid-column-one-half">
            <FormGroup
              error={
                formState.errors.services?.covidAges?.message ??
                formState.errors.services?.fluAges?.message ??
                formState.errors.services?.shinglesAges?.message ??
                formState.errors.services?.pneumoniaAges?.message ??
                formState.errors.services?.rsvAges?.message ??
                formState.errors.services?.message
              }
            >
              <label
                className="nhsuk-label nhsuk-checkboxes__label"
                htmlFor="services"
                id={'services-offered-in-session'}
                style={{ paddingLeft: 0 }}
              >
                Services available
              </label>
              <CheckBox
                label={'Covid'}
                {...register('services.covid')}
              ></CheckBox>
              {covidWatch && (
                <Controller
                  name="services.covidAges"
                  control={control}
                  rules={{
                    validate: covidAges => {
                      if (covidWatch && covidAges.length === 0) {
                        return 'A clinic must serve at least one age group for each service offered (covid)';
                      }
                      return true;
                    },
                  }}
                  render={({ field }) => (
                    <div className="nhsuk-checkboxes-custom__conditional">
                      <CheckBoxes>
                        <CheckBox
                          value="select-all"
                          label="Select All"
                          checked={field.value.includes('select-all')}
                          onChange={() => {
                            if (covidAgesWatch.length >= covidServices.length) {
                              setValue('services.covidAges', []);
                            } else {
                              setValue('services.covidAges', [
                                ...covidServices.map(service => service.id),
                                'select-all',
                              ]);
                            }
                            trigger('services.covidAges');
                          }}
                        />
                        {covidServices.map((service, index) => {
                          return (
                            <CheckBox
                              key={`covid-services-${index}`}
                              value={service.id}
                              label={service.displayName}
                              checked={field.value.includes(service.id)}
                              onChange={e => {
                                const newValue = e.target.checked
                                  ? [...field.value, service.id]
                                  : field.value.filter(
                                      val =>
                                        val !== service.id &&
                                        val !== 'select-all',
                                    );
                                field.onChange(newValue);
                              }}
                            />
                          );
                        })}
                      </CheckBoxes>
                    </div>
                  )}
                />
              )}

              <CheckBox label={'Flu'} {...register('services.flu')}></CheckBox>
              {fluWatch && (
                <Controller
                  name="services.fluAges"
                  control={control}
                  rules={{
                    validate: fluAges => {
                      if (fluWatch && fluAges.length === 0) {
                        return 'A clinic must serve at least one age group for each service offered (flu)';
                      }
                      return true;
                    },
                  }}
                  render={({ field }) => (
                    <div className="nhsuk-checkboxes-custom__conditional">
                      <CheckBoxes>
                        <CheckBox
                          value="select-all"
                          label="Select All"
                          checked={field.value.includes('select-all')}
                          onChange={() => {
                            if (fluAgesWatch.length >= fluServices.length) {
                              setValue('services.fluAges', []);
                            } else {
                              setValue('services.fluAges', [
                                ...fluServices.map(service => service.id),
                                'select-all',
                              ]);
                            }
                            trigger('services.fluAges');
                          }}
                        />
                        {fluServices.map((service, index) => {
                          return (
                            <CheckBox
                              key={`flu-services-${index}`}
                              value={service.id}
                              label={service.displayName}
                              checked={field.value.includes(service.id)}
                              onChange={e => {
                                const newValue = e.target.checked
                                  ? [...field.value, service.id]
                                  : field.value.filter(
                                      val =>
                                        val !== service.id &&
                                        val !== 'select-all',
                                    );
                                field.onChange(newValue);
                              }}
                            />
                          );
                        })}
                      </CheckBoxes>
                    </div>
                  )}
                />
              )}

              <CheckBox
                label={'Shingles'}
                {...register('services.shingles')}
              ></CheckBox>
              {shinglesWatch && (
                <Controller
                  name="services.shinglesAges"
                  control={control}
                  rules={{
                    validate: shinglesAges => {
                      if (shinglesWatch && shinglesAges.length === 0) {
                        return 'A clinic must serve at least one age group for each service offered (shingles)';
                      }
                      return true;
                    },
                  }}
                  render={({ field }) => (
                    <div className="nhsuk-checkboxes-custom__conditional">
                      <CheckBoxes>
                        <CheckBox
                          value="select-all"
                          label="Select All"
                          checked={field.value.includes('select-all')}
                          onChange={() => {
                            if (
                              shinglesAgesWatch.length >=
                              shinglesServices.length
                            ) {
                              setValue('services.shinglesAges', []);
                            } else {
                              setValue('services.shinglesAges', [
                                ...shinglesServices.map(service => service.id),
                                'select-all',
                              ]);
                            }
                            trigger('services.shinglesAges');
                          }}
                        />
                        {shinglesServices.map((service, index) => {
                          return (
                            <CheckBox
                              key={`shingles-services-${index}`}
                              value={service.id}
                              label={service.displayName}
                              checked={field.value.includes(service.id)}
                              onChange={e => {
                                const newValue = e.target.checked
                                  ? [...field.value, service.id]
                                  : field.value.filter(
                                      val =>
                                        val !== service.id &&
                                        val !== 'select-all',
                                    );
                                field.onChange(newValue);
                              }}
                            />
                          );
                        })}
                      </CheckBoxes>
                    </div>
                  )}
                />
              )}

              <CheckBox
                label={'Pneumonia'}
                {...register('services.pneumonia')}
              ></CheckBox>
              {pneumoniaWatch && (
                <Controller
                  name="services.pneumoniaAges"
                  control={control}
                  rules={{
                    validate: pneumoniaAges => {
                      if (pneumoniaWatch && pneumoniaAges.length === 0) {
                        return 'A clinic must serve at least one age group for each service offered (pneumonia)';
                      }
                      return true;
                    },
                  }}
                  render={({ field }) => (
                    <div className="nhsuk-checkboxes-custom__conditional">
                      <CheckBoxes>
                        <CheckBox
                          value="select-all"
                          label="Select All"
                          checked={field.value.includes('select-all')}
                          onChange={() => {
                            if (
                              pneumoniaAgesWatch.length >=
                              pneumoniaServices.length
                            ) {
                              setValue('services.pneumoniaAges', []);
                            } else {
                              setValue('services.pneumoniaAges', [
                                ...pneumoniaServices.map(service => service.id),
                                'select-all',
                              ]);
                            }
                            trigger('services.pneumoniaAges');
                          }}
                        />
                        {pneumoniaServices.map((service, index) => {
                          return (
                            <CheckBox
                              key={`pneumonia-services-${index}`}
                              value={service.id}
                              label={service.displayName}
                              checked={field.value.includes(service.id)}
                              onChange={e => {
                                const newValue = e.target.checked
                                  ? [...field.value, service.id]
                                  : field.value.filter(
                                      val =>
                                        val !== service.id &&
                                        val !== 'select-all',
                                    );
                                field.onChange(newValue);
                              }}
                            />
                          );
                        })}
                      </CheckBoxes>
                    </div>
                  )}
                />
              )}

              <CheckBox label={'RSV'} {...register('services.rsv')}></CheckBox>
              {rsvWatch && (
                <Controller
                  name="services.rsvAges"
                  control={control}
                  rules={{
                    validate: rsvAges => {
                      if (rsvWatch && rsvAges.length === 0) {
                        return 'A clinic must serve at least one age group for each service offered (rsv)';
                      }
                      return true;
                    },
                  }}
                  render={({ field }) => (
                    <div className="nhsuk-checkboxes-custom__conditional">
                      <CheckBoxes>
                        <CheckBox
                          value="select-all"
                          label="Select All"
                          checked={field.value.includes('select-all')}
                          onChange={() => {
                            if (rsvAgesWatch.length >= rsvServices.length) {
                              setValue('services.rsvAges', []);
                            } else {
                              setValue('services.rsvAges', [
                                ...rsvServices.map(service => service.id),
                                'select-all',
                              ]);
                            }
                            trigger('services.rsvAges');
                          }}
                        />
                        {rsvServices.map((service, index) => {
                          return (
                            <CheckBox
                              key={`rsv-services-${index}`}
                              value={service.id}
                              label={service.displayName}
                              checked={field.value.includes(service.id)}
                              onChange={e => {
                                const newValue = e.target.checked
                                  ? [...field.value, service.id]
                                  : field.value.filter(
                                      val =>
                                        val !== service.id &&
                                        val !== 'select-all',
                                    );
                                field.onChange(newValue);
                              }}
                            />
                          );
                        })}
                      </CheckBoxes>
                    </div>
                  )}
                />
              )}
            </FormGroup>
          </div>
        </div>

        <AppointmentsSummaryText
          total={Math.max(
            Math.floor(
              (60 / appointmentLengthWatch) *
                maxSimultaneousAppointmentsWatch *
                hoursBetween(startTimeWatch, endTimeWatch),
            ),
            0,
          )}
          perHour={Math.floor(
            (60 / appointmentLengthWatch) * maxSimultaneousAppointmentsWatch,
          )}
        />

        <Button
          type="submit"
          styleType="secondary"
          style={{ marginBottom: 16 }}
        >
          Add session
        </Button>
      </Card>
    </form>
  );
};

export default AddSessionForm;
