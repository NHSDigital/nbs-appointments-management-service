'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  Card,
  SmallSpinnerWithText,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { formatTimeString, parseDateComponents } from '@services/timeService';
import { useFormContext } from 'react-hook-form';
import {
  CreateAvailabilityFormValues,
  services,
} from './availability-template-wizard';

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
        services.find(service => service.value === serviceValue)?.label,
    )
    .join(', ');

  const summary: SummaryListItem[] =
    sessionType === 'single'
      ? [
          {
            title: 'Date',
            value: `${parseDateComponents(startDate)?.format('D MMMM YYYY')}`,
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
          {
            title: 'Maximum simultaneous appointments',
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
            title: 'Appointment length in minutes',
            value: `${session.slotLength}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },
        ]
      : [
          {
            title: 'Dates',
            value: `${parseDateComponents(startDate)?.format('D MMMM YYYY')} - ${parseDateComponents(endDate)?.format('D MMMM YYYY')}`,
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
          {
            title: 'Maximum simultaneous appointments',
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
            title: 'Appointment length in minutes',
            value: `${session.slotLength}`,
            action: {
              renderingStrategy: 'client',
              text: 'Change',
              onClick: () => {
                setCurrentStep(4);
              },
            },
          },
        ];

  return (
    <>
      {stepNumber === 1 ? (
        <BackLink
          href={returnRouteUponCancellation ?? '/'}
          renderingStrategy="server"
        />
      ) : (
        <BackLink onClick={goToPreviousStep} renderingStrategy="client" />
      )}
      <NhsHeading
        title="Check availability period"
        caption="Create availability period"
      />
      <Card title={'Session details'}>
        <SummaryList items={summary}></SummaryList>
      </Card>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <Button type="submit">Save</Button>
      )}
    </>
  );
};

export default SummaryStep;
