'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import StartAndEndDateStep from './start-and-end-date-step';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import {
  DateComponents,
  Session,
  Site,
  DayOfWeek,
  ClinicalService,
} from '@types';
import SingleOrRepeatingSessionStep from './single-or-repeating-session-step';
import SummaryStep from './summary-step';
import saveAvailabilityTemplate from './save-availability-template';
import { useRouter } from 'next/navigation';
import TimeAndCapacityStep from './time-and-capacity-step';
import DaysOfWeekStep from './days-of-week-step';
import SelectServicesStep from './select-services-step';
import { ChangeEvent } from 'react';
import { RFC3339Format, parseToUkDatetime } from '@services/timeService';

export type CreateAvailabilityFormValues = {
  startDate: DateComponents;
  endDate: DateComponents;
  sessionType: 'single' | 'repeating';
  days: DayOfWeek[];
  session: Session;
};

type Props = {
  site: Site;
  clinicalServices: ClinicalService[];
  date?: string;
};

export const handlePositiveBoundedNumberInput = (
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

const AvailabilityTemplateWizard = ({
  site,
  date,
  clinicalServices,
}: Props) => {
  const startDate = date ? parseToUkDatetime(date, RFC3339Format) : undefined;
  const methods = useForm<CreateAvailabilityFormValues>({
    defaultValues: {
      days: [],
      sessionType: 'single',
      session: {
        break: 'no',
        services: [],
      },
      startDate: startDate
        ? {
            year: startDate.year(),
            month: startDate.month() + 1,
            day: startDate.date(),
          }
        : undefined,
    },
  });
  const router = useRouter();

  const submitForm: SubmitHandler<CreateAvailabilityFormValues> = async (
    form: CreateAvailabilityFormValues,
  ) => {
    await saveAvailabilityTemplate(form, site);

    // TODO: This redirect is blocked by awaiting the submit request, which could cause a delay to users.
    // We need to handle this better, potentially with a loading spinner or something
    // Maybe a <Suspense> object

    date
      ? router.push(`/site/${site.id}/view-availability/week?date=${date}`)
      : router.push(`/site/${site.id}/create-availability`);
  };

  const returnToWeekView = () => {
    router.replace(`/site/${site.id}/view-availability/week?date=${date}`);
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        <Wizard
          id="create-availability-wizard"
          initialStep={date ? 4 : 1}
          returnRouteUponCancellation={`/site/${site.id}/create-availability`}
          onCompleteFinalStep={() => {
            methods.handleSubmit(submitForm);
          }}
        >
          <WizardStep>
            {stepProps => <SingleOrRepeatingSessionStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => <StartAndEndDateStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => <DaysOfWeekStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <TimeAndCapacityStep
                {...stepProps}
                goToPreviousStepOverride={date ? returnToWeekView : undefined}
              />
            )}
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <SelectServicesStep
                {...stepProps}
                clinicalServices={clinicalServices}
              />
            )}
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <SummaryStep {...stepProps} clinicalServices={clinicalServices} />
            )}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default AvailabilityTemplateWizard;
