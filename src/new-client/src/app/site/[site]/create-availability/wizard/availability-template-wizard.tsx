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

export type CreateAvailabilityFormValues = {
  startDate: DateComponents;
  endDate: DateComponents;
  sessionType: 'single' | 'repeating';
  days: DayOfWeek[];
  session: Session;
};

type Props = {
  site: Site;
};

const AvailabilityTemplateWizard = ({ site }: Props) => {
  const methods = useForm<CreateAvailabilityFormValues>({
    defaultValues: {
      days: [],
      sessionType: 'single',
      session: {
        startTime: {
          hour: 9,
          minute: 0,
        },
        endTime: {
          hour: 17,
          minute: 0,
        },
        break: 'no',
        capacity: 1,
        slotLength: 5,
        services: [],
      },
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
    router.push(`/site/${site.id}/create-availability`);
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
