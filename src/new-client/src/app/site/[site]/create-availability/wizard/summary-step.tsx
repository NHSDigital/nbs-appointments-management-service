'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  ButtonGroup,
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
      action: {
        href: '#',
        text: 'Change',
      },
    },
    {
      title: 'Days',
      value: `Monday, Tuesday, Wednesday, Thursday, Friday`,
      action: {
        href: '#',
        text: 'Change',
      },
    },
    {
      title: 'Time',
      value: `09:00 - 17:00`,
      action: {
        href: '#',
        text: 'Change',
      },
    },
    {
      title: 'Breaks',
      value: `12:00 - 13:00`,
      action: {
        href: '#',
        text: 'Change',
      },
    },
    {
      title: 'Services available',
      value: `Flu 65+, RSV 75+`,
      action: {
        href: '#',
        text: 'Change',
      },
    },
    {
      title: 'Maximum simultaneous appointments',
      value: `1`,
      action: {
        href: '#',
        text: 'Change',
      },
    },
    {
      title: 'Appointment length in minutes',
      value: `10`,
      action: {
        href: '#',
        text: 'Change',
      },
    },
  ];

  return (
    <>
      <BackLink onClick={goToPreviousStep} href="" />
      <NhsHeading
        title="Check availability period"
        caption="Create availability period"
      />
      <Card title={'Session details'}>
        <SummaryList items={summary}></SummaryList>
      </Card>
      <ButtonGroup>
        <Button type="submit">Save without publishing</Button>
        <Button type="submit" styleType="secondary">
          Save without publishing
        </Button>
      </ButtonGroup>
    </>
  );
};

export default SingleOrRepeatingSessionStep;
