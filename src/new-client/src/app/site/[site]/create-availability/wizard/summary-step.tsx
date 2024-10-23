'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  Card,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { parseAndValidateDateFromComponents } from '@services/timeService';
import { useFormContext } from 'react-hook-form';
import { AvailabilityTemplateFormValues } from './availability-template-wizard';

const SingleOrRepeatingSessionStep = ({
  goToPreviousStep,
}: InjectedWizardProps) => {
  const { getValues } = useFormContext<AvailabilityTemplateFormValues>();

  const formValues = getValues();

  const startDate = parseAndValidateDateFromComponents(
    formValues.startDateDay,
    formValues.startDateMonth,
    formValues.startDateYear,
  );

  const endDate = parseAndValidateDateFromComponents(
    formValues.endDateDay,
    formValues.endDateMonth,
    formValues.endDateYear,
  );

  const summary: SummaryListItem[] = [
    {
      title: 'Dates',
      value: `${startDate?.format('D MMMM YYYY')} - ${endDate?.format('D MMMM YYYY')}`,
    },
    {
      title: 'Days',
      value: `Monday, Tuesday, Wednesday, Thursday, Friday`,
    },
    {
      title: 'Time',
      value: `09:00 - 17:00`,
    },
    {
      title: 'Breaks',
      value: `12:00 - 13:00`,
    },
    {
      title: 'Services available',
      value: `Flu 65+, RSV 75+`,
    },
    {
      title: 'Maximum simultaneous appointments',
      value: `1`,
    },
    {
      title: 'Appointment length in minutes',
      value: `10`,
    },
  ];

  return (
    <>
      <BackLink onClick={goToPreviousStep} renderingStrategy="client" />
      <NhsHeading
        title="Check availability period"
        caption="Create availability period"
      />
      <Card title={'Session details'}>
        <SummaryList items={summary}></SummaryList>
      </Card>
      <Button type="submit">Save</Button>
    </>
  );
};

export default SingleOrRepeatingSessionStep;
