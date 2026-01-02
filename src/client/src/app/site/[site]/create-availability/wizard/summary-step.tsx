'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  SmallSpinnerWithText,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import {
  toTimeFormat,
  parseDateComponentsToUkDatetime,
} from '@services/timeService';
import { useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { ClinicalService } from '@types';

type SummaryStepProps = {
  clinicalServices: ClinicalService[];
};

const SummaryStep = ({
  setCurrentStep,
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
  clinicalServices,
  pendingSubmit,
}: InjectedWizardProps & SummaryStepProps) => {
  const { getValues } = useFormContext<CreateAvailabilityFormValues>();

  const { startDate, endDate, session, days, sessionType } = getValues();

  const servicesText = session.services
    .map(
      serviceValue =>
        clinicalServices.find(service => service.value === serviceValue)
          ?.label ?? serviceValue,
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
            value: `${toTimeFormat(session.startTime)} - ${toTimeFormat(session.endTime)}`,
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
            value: `${toTimeFormat(session.startTime)} - ${toTimeFormat(session.endTime)}`,
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
      <NhsHeading caption="Add availability" title="Check your answers" />
      <SummaryList items={summary}></SummaryList>

      <h2 className="nhsuk-heading-m nhsuk-u-margin-top-6">
        Before you continue
      </h2>
      <p>
        By publishing this availability, you confirm that your site is assured
        to deliver the services you have selected.
      </p>

      {pendingSubmit ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <Button type="submit">Save and publish availability</Button>
      )}
    </>
  );
};

export default SummaryStep;
