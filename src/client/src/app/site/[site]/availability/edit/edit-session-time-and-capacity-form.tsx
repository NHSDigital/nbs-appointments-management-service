'use client';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Site, SessionSummary, Session, AvailabilitySession } from '@types';
import { useRouter } from 'next/navigation';
import { editSession } from '@services/appointmentsService';
import {
  Button,
  FormGroup,
  InsetText,
  SmallSpinnerWithText,
  TextInput,
} from '@components/nhsuk-frontend';
import { Controller } from 'react-hook-form';
import {
  compareTimes,
  dateTimeFormat,
  parseToUkDatetime,
  parseToTimeComponents,
  toTimeFormat,
  isValidStartTime,
  parseDateAndTimeComponentsToUkDateTime,
} from '@services/timeService';
import { ChangeEvent, useTransition } from 'react';
import { sessionLengthInMinutes } from '@services/availabilityCalculatorService';
import fromServer from '@server/fromServer';

export type EditSessionFormValues = {
  sessionToEdit: Session;
  newSession: Session;
};

type Props = {
  date: string;
  site: Site;
  existingSession: SessionSummary;
  changeSessionFlag: boolean;
};

const EditSessionTimeAndCapacityForm = ({
  site,
  existingSession,
  date,
  changeSessionFlag,
}: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const existingUkStartTime = parseToUkDatetime(
    existingSession.ukStartDatetime,
    dateTimeFormat,
  ).format('HH:mm');
  const existingUkEndTime = parseToUkDatetime(
    existingSession.ukEndDatetime,
    dateTimeFormat,
  ).format('HH:mm');

  const {
    handleSubmit,
    watch,
    control,
    formState: { errors },
  } = useForm<EditSessionFormValues>({
    defaultValues: {
      sessionToEdit: {
        startTime: parseToTimeComponents(existingUkStartTime),
        endTime: parseToTimeComponents(existingUkEndTime),
        services: Object.keys(
          existingSession.totalSupportedAppointmentsByService,
        ).map(service => service),
        slotLength: existingSession.slotLength,
        capacity: existingSession.capacity,
      },
      newSession: {
        startTime: {
          hour: existingUkStartTime.split(':')[0],
          minute: existingUkStartTime.split(':')[1],
        },
        endTime: {
          hour: existingUkEndTime.split(':')[0],
          minute: existingUkEndTime.split(':')[1],
        },
        services: Object.keys(
          existingSession.totalSupportedAppointmentsByService,
        ).map(service => service),
        slotLength: existingSession.slotLength,
        capacity: existingSession.capacity,
      },
    },
  });

  const sessionToEditWatch = watch('sessionToEdit');
  const router = useRouter();

  const submitForm: SubmitHandler<EditSessionFormValues> = async (
    form: EditSessionFormValues,
  ) => {
    startTransition(async () => {
      if (changeSessionFlag) {
        const reroute = `/site/${site.id}/availability/edit/confirmation?session=${btoa(JSON.stringify(existingSession))}&date=${date}`;
        router.push(reroute);
      } else {
        const updatedSession = toAvailabilitySession(form.newSession);

        const sessionStart = parseDateAndTimeComponentsToUkDateTime(
          date,
          form.newSession.startTime,
        );
        const sessionEnd = parseDateAndTimeComponentsToUkDateTime(
          date,
          form.newSession.endTime,
        );

        const validSessionStartTime = isValidStartTime(
          sessionStart,
          sessionEnd,
          form.newSession.slotLength,
        );

        if (
          existingSession.totalSupportedAppointments === 0 ||
          validSessionStartTime
        ) {
          await updateSession(form, updatedSession);
          return;
        }

        const updatedString = btoa(JSON.stringify(updatedSession));
        const existingString = btoa(JSON.stringify(existingSession));

        router.push(
          `edit/edit-start-time?date=${date}&existingSession=${existingString}&updatedSession=${updatedString}`,
        );
      }
    });
  };

  const updateSession = async (
    form: EditSessionFormValues,
    updatedSession: AvailabilitySession,
  ) => {
    await fromServer(
      editSession({
        date,
        site: site.id,
        mode: 'Edit',
        sessions: [updatedSession],
        sessionToEdit: toAvailabilitySession(form.sessionToEdit),
      }),
    );

    router.push(
      `edit/confirmed?updatedSession=${btoa(JSON.stringify(updatedSession))}&date=${date}`,
    );
  };

  const toAvailabilitySession = (session: Session): AvailabilitySession => ({
    from: toTimeFormat(session.startTime) ?? '',
    until: toTimeFormat(session.endTime) ?? '',
    slotLength: session.slotLength,
    capacity: session.capacity,
    services: session.services,
  });

  const handleTwoDigitPositiveBoundedNumberInput = (
    e: ChangeEvent<HTMLInputElement>,
    upperBound: number,
  ) => {
    const asNumber = Number(e.currentTarget.value);
    if (asNumber < 0 || Number.isNaN(asNumber) || !Number.isInteger(asNumber)) {
      return '00';
    }

    if (asNumber > upperBound) {
      return `0${e.target.value.slice(-1)}`;
    }

    switch (e.target.value.length) {
      case 0:
        return `00`;
      case 1:
        return `0${e.target.value}`;
      case 2:
        return e.target.value;
      default:
        return e.target.value.slice(-2);
    }
  };

  const handlePositiveBoundedNumberInput = (
    e: ChangeEvent<HTMLInputElement>,
    upperBound: number,
  ) => {
    const asNumber = Number(e.currentTarget.value);
    if (asNumber < 1 || Number.isNaN(asNumber) || !Number.isInteger(asNumber)) {
      return '';
    }

    if (asNumber > upperBound) {
      return upperBound;
    }

    return asNumber;
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <InsetText>
        <p>
          {existingSession.totalSupportedAppointments} booked appointments in
          this session.
          <br />
          {existingSession.maximumCapacity -
            existingSession.totalSupportedAppointments}{' '}
          unbooked appointments in this session.
        </p>
      </InsetText>
      <FormGroup
        legend="Session times"
        hint="For example, 14:30"
        error={
          errors.newSession?.startTime?.message ||
          errors.newSession?.endTime?.message
        }
      >
        <Controller
          name={'newSession.startTime'}
          control={control}
          rules={{
            validate: value => {
              if (toTimeFormat(value) === undefined) {
                return 'Enter a valid start time';
              }

              if (
                compareTimes(value, sessionToEditWatch.startTime) == 'earlier'
              ) {
                return 'Enter a start or end time that reduces the length of this session.';
              }
            },
          }}
          render={() => (
            <>
              <div className="nhsuk-label">Start time</div>
              <div className="nhsuk-time-input-custom">
                <Controller
                  control={control}
                  name="newSession.startTime.hour"
                  render={({ field }) => (
                    <div className="nhsuk-time-input-custom__item">
                      <label
                        id="start-time-accessibility-label-hour"
                        htmlFor="start-time-hour"
                      >
                        Session start time - hour
                      </label>
                      <input
                        aria-labelledby="start-time-accessibility-label-hour"
                        id="start-time-hour"
                        className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 23),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />

                <div className="nhsuk-time-input-custom__item">
                  <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
                    :
                  </div>
                </div>

                <Controller
                  control={control}
                  name="newSession.startTime.minute"
                  render={({ field }) => (
                    <div className="nhsuk-time-input-custom__item">
                      <label
                        id="start-time-accessibility-label-minute"
                        htmlFor="start-time-minute"
                      >
                        Session start time - minute
                      </label>
                      <input
                        aria-labelledby="start-time-accessibility-label-minute"
                        className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
                        id="start-time-minute"
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 59),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />
              </div>
            </>
          )}
        />
        <Controller
          name={'newSession.endTime'}
          control={control}
          rules={{
            validate: (value, form) => {
              const endTime = toTimeFormat(value);
              const startTime = toTimeFormat(form.newSession.startTime);
              if (endTime === undefined || startTime === undefined) {
                return 'Enter a valid end time';
              }

              const minutesBetweenStartAndEnd = sessionLengthInMinutes(
                form.newSession.startTime,
                value,
              );

              if (minutesBetweenStartAndEnd < 0) {
                return 'Session end time must be after the start time';
              }

              if (minutesBetweenStartAndEnd <= 5) {
                return 'Session length must be more than 5 minutes';
              }

              if (compareTimes(value, sessionToEditWatch.endTime) == 'later') {
                return 'Enter a start or end time that reduces the length of this session.';
              }
            },
          }}
          render={() => (
            <>
              <div className="nhsuk-label">End time</div>
              <div className="nhsuk-time-input-custom">
                <Controller
                  control={control}
                  name="newSession.endTime.hour"
                  render={({ field }) => (
                    <div className="nhsuk-time-input-custom__item">
                      <label
                        id="end-time-accessibility-label-hour"
                        htmlFor="end-time-hour"
                      >
                        Session end time - hour
                      </label>
                      <input
                        aria-labelledby="end-time-accessibility-label-hour"
                        className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
                        id="end-time-hour"
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 23),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />

                <div className="nhsuk-time-input-custom__item">
                  <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
                    :
                  </div>
                </div>

                <Controller
                  control={control}
                  name="newSession.endTime.minute"
                  render={({ field }) => (
                    <div className="nhsuk-time-input-custom__item">
                      <label
                        id="end-time-accessibility-label-minute"
                        htmlFor="end-time-minute"
                      >
                        Session end time - minute
                      </label>
                      <input
                        aria-labelledby="end-time-accessibility-label-minute"
                        className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
                        id="end-time-minute"
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 59),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />
              </div>
            </>
          )}
        />
      </FormGroup>
      <br />
      <FormGroup legend="Capacity">
        <Controller
          control={control}
          name="newSession.capacity"
          rules={{
            required: { value: true, message: 'Capacity is required' },
            min: { value: 1, message: 'Capacity must be at least 1' },
            validate: value => {
              const candidate = Number(value);
              if (!Number.isInteger(candidate)) {
                return 'Capacity must be a whole number';
              }
              if (candidate > 99) {
                return 'Capacity must be less than 100';
              }
              if (candidate > existingSession.capacity) {
                return 'Enter a number that reduces the vaccinators or vaccinator spaces in this session.';
              }
            },
          }}
          render={({ field }) => (
            <FormGroup
              legend="How many vaccinators or vaccination spaces do you have?"
              error={errors.newSession?.capacity?.message}
            >
              <label
                id="capacity"
                htmlFor="capacity"
                style={{ display: 'none' }}
              >
                How many vaccinators or vaccination spaces do you have?
              </label>
              <TextInput
                id="capacity"
                aria-labelledby="capacity"
                inputMode="numeric"
                type="number"
                width={2}
                onChange={e =>
                  field.onChange(handlePositiveBoundedNumberInput(e, 99))
                }
                value={field.value ?? ''}
              />
            </FormGroup>
          )}
        />
      </FormGroup>

      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Continue</Button>
      )}
    </form>
  );
};

export default EditSessionTimeAndCapacityForm;
