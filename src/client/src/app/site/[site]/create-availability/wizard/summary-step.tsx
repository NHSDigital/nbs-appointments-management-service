'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  InsetText,
  SmallSpinnerWithText,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import {
  formatTimeString,
  parseDateComponentsToUkDatetime,
} from '@services/timeService';
import { useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { calculateCapacity } from './capacity-calculation';
import { clinicalServices } from '@types';

const SummaryStep = ({
  setCurrentStep,
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
}: InjectedWizardProps) => {
  const {
    getValues,
    formState: { isSubmitting, isSubmitSuccessful },
  } = useFormContext<CreateAvailabilityFormValues>();

  const { startDate, endDate, session, days, sessionType } = getValues();

  const servicesText = session.services
    .map(
      serviceValue =>
        clinicalServices.find(service => service.value === serviceValue)?.label,
    )
    .join(', ');

  const summary: SummaryListItem[] =
    sessionType === 'single'
      ? [
          {
            title: 'Date',
            value: `${parseDateComponentsToUkDatetime(startDate)?.format('D MMMM YYYY')}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(2);
              },
            },
          },
          {
            title: 'Time',
            value: `${formatTimeString(session.startTime)} - ${formatTimeString(session.endTime)}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },

          {
            title: 'Vaccinators or vaccination spaces available',
            value: `${session.capacity}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },
          {
            title: 'Appointment length',
            value: `${session.slotLength} minutes`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },
          {
            title: 'Services available',
            value: servicesText,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(5);
              },
            },
          },
        ]
      : [
          {
            title: 'Dates',
            value: `${parseDateComponentsToUkDatetime(startDate)?.format('D MMMM YYYY')} - ${parseDateComponentsToUkDatetime(endDate)?.format('D MMMM YYYY')}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(2);
              },
            },
          },
          {
            title: 'Days',
            value: days.join(', '),
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(3);
              },
            },
          },
          {
            title: 'Time',
            value: `${formatTimeString(session.startTime)} - ${formatTimeString(session.endTime)}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },
          {
            title: 'Vaccinators or vaccination spaces available',
            value: `${session.capacity}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },
          {
            title: 'Appointment length',
            value: `${session.slotLength} minutes`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },
          {
            title: 'Services available',
            value: servicesText,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(5);
              },
            },
          },
        ];

  const capacity = calculateCapacity({
    startTime: session.startTime,
    endTime: session.endTime,
    slotLength: session.slotLength,
    capacity: session.capacity,
  });

  return (
    <>
      {stepNumber === 1 ? (
        <BackLink
          href={returnRouteUponCancellation ?? '/sites'}
          renderingStrategy="server"
          text="Go back"
        />
      ) : (
        <BackLink
          onClick={goToPreviousStep}
          renderingStrategy="client"
          text="Go back"
        />
      )}
      <NhsHeading
        title={
          sessionType === 'single'
            ? 'Check single date session'
            : 'Check weekly session'
        }
      />
      <SummaryList items={summary}></SummaryList>

      <p>
        <strong>{capacity.appointmentsPerSession}</strong> total appointments in
        the session
        {capacity.appointmentsPerHour !== undefined && (
          <>
            <br />
            Up to <strong>{capacity.appointmentsPerHour}</strong> appointments
            per hour
          </>
        )}
        <br />
      </p>

      <InsetText>
        Saving will allow people to book appointments for the availability
        you've created. Make sure the information is accurate before saving.
      </InsetText>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <Button type="submit">Save session</Button>
      )}
    </>
  );
};

export default SummaryStep;
