'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import StartAndEndDateStep from './start-and-end-date-step';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { DateComponents, Session, Site, DayOfWeek } from '@types';
import SingleOrRepeatingSessionStep from './single-or-repeating-session-step';
import SummaryStep from './summary-step';
import saveAvailabilityTemplate from './save-availability-template';
import { useRouter } from 'next/navigation';
import TimeAndCapacityStep from './time-and-capacity-step';
import DaysOfWeekStep from './days-of-week-step';
import SelectServicesStep from './select-services-step';
import { ChangeEvent } from 'react';
import dayjs from 'dayjs';

export type CreateAvailabilityFormValues = {
  startDate: DateComponents;
  endDate: DateComponents;
  sessionType: 'single' | 'repeating';
  days: DayOfWeek[];
  session: Session;
};

type Props = {
  site: Site;
  date: string | null;
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

const AvailabilityTemplateWizard = ({ site, date }: Props) => {
  const startDate = dayjs(date, 'YYYY-MM-DD');
  const methods = useForm<CreateAvailabilityFormValues>({
    defaultValues: {
      days: [],
      sessionType: 'single',
      session: {
        break: 'no',
        services: [],
      },
      startDate: date
        ? {
            year: startDate.year(),
            month: startDate.month() + 1,
            day: startDate.date(),
          }
        : undefined,
    },
  });
  const router = useRouter();
  const dateIsNotProvided = () => {
    return typeof date !== 'string';
  };

  const submitForm: SubmitHandler<CreateAvailabilityFormValues> = async (
    form: CreateAvailabilityFormValues,
  ) => {
    await saveAvailabilityTemplate(form, site);

    // TODO: This redirect is blocked by awaiting the submit request, which could cause a delay to users.
    // We need to handle this better, potentially with a loading spinner or something
    // Maybe a <Suspense> object

    dateIsNotProvided()
      ? router.push(`/site/${site.id}/create-availability`)
      : router.push(`/site/${site.id}/view-availability/week?date=${date}`);
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        <Wizard
          id="create-availability-wizard"
          initialStep={1}
          returnRouteUponCancellation={`/site/${site.id}/create-availability`}
          onCompleteFinalStep={() => {
            methods.handleSubmit(submitForm);
          }}
        >
          {dateIsNotProvided() && (
            <WizardStep>
              {stepProps => <SingleOrRepeatingSessionStep {...stepProps} />}
            </WizardStep>
          )}
          {dateIsNotProvided() && (
            <WizardStep>
              {stepProps => <StartAndEndDateStep {...stepProps} />}
            </WizardStep>
          )}
          {dateIsNotProvided() && (
            <WizardStep>
              {stepProps => <DaysOfWeekStep {...stepProps} />}
            </WizardStep>
          )}
          <WizardStep>
            {stepProps => <TimeAndCapacityStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => <SelectServicesStep {...stepProps} />}
          </WizardStep>
          <WizardStep>{stepProps => <SummaryStep {...stepProps} />}</WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default AvailabilityTemplateWizard;
