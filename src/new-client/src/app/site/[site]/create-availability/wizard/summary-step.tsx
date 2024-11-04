'use client';
import NhsHeading from '@components/nhs-heading';
import {
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

const SummaryStep = ({}: InjectedWizardProps) => {
  const {
    getValues,
    formState: { isSubmitting, isSubmitSuccessful },
  } = useFormContext<CreateAvailabilityFormValues>();

  const { startDate, endDate, session, days, sessionType } = getValues();

  const datesText =
    sessionType === 'repeating'
      ? `${parseDateComponents(startDate)?.format('D MMMM YYYY')} - ${parseDateComponents(endDate)?.format('D MMMM YYYY')}`
      : `${parseDateComponents(startDate)?.format('D MMMM YYYY')}`;

  const servicesText = session.services
    .map(
      serviceValue =>
        services.find(service => service.value === serviceValue)?.label,
    )
    .join(', ');

  const summary: SummaryListItem[] = [
    {
      title: 'Dates',
      value: datesText,
    },
    {
      title: 'Days',
      value: days.join(', '),
    },
    {
      title: 'Time',
      value: `${formatTimeString(session.startTime)} - ${formatTimeString(session.endTime)}`,
    },
    {
      title: 'Services available',
      value: servicesText,
    },
    {
      title: 'Maximum simultaneous appointments',
      value: `${session.capacity}`,
    },
    {
      title: 'Appointment length in minutes',
      value: `${session.slotLength}`,
    },
  ];

  return (
    <>
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
