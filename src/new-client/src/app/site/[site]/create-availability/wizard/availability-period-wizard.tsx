'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import StartAndEndDateStep from './start-and-end-date-step';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { AvailabilityPeriod, Site, UserProfile } from '@types';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import saveAvailabilityPeriod from './save-availability-period';
import SingleOrRepeatingSessionStep from './single-or-repeating-session-step';

export type AvailabilityPeriodFormValues = {
  startDateDay: number;
  startDateMonth: number;
  startDateYear: number;
  endDateDay: number;
  endDateMonth: number;
  endDateYear: number;
};

type AvailabilityPeriodWizardState = {
  initialStep: number;
  activeStep: number;
};

type Props = {
  site: Site;
};

const AvailabilityPeriodWizard = ({ site }: Props) => {
  //   const wizardState = useState<AvailabilityPeriodWizardState>({
  //     initialStep: 1,
  //     activeStep: 1,
  //   });
  const methods = useForm<AvailabilityPeriodFormValues>();

  const submitForm: SubmitHandler<AvailabilityPeriodFormValues> = (
    form: AvailabilityPeriodFormValues,
  ) => {
    const availabilityPeriod = mapFormValuesToAvailabilityPeriod(form);
    saveAvailabilityPeriod(availabilityPeriod);
  };
  const { push } = useRouter();

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        <Wizard
          id="create-availability-wizard"
          initialStep={1}
          onCancelOutOfWizard={() => {
            console.log('Cancelled');
            push(`/site/${site}/create-availability`);
          }}
          transitionToOnCancel={`/site/${site.id}/create-availability`}
          onCompleteFinalStep={() => {
            methods.handleSubmit(submitForm);
          }}
        >
          <WizardStep>
            {stepProps => <StartAndEndDateStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => <SingleOrRepeatingSessionStep {...stepProps} />}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

const mapFormValuesToAvailabilityPeriod = (
  formValues: AvailabilityPeriodFormValues,
): AvailabilityPeriod => {
  // TODO: Implement this properly and use DayJS, plus validation etc. etc.
  return {
    startDate: new Date(
      formValues.startDateYear,
      formValues.startDateMonth,
      formValues.startDateDay,
    ),
    endDate: new Date(
      formValues.endDateYear,
      formValues.endDateMonth,
      formValues.endDateDay,
    ),
    services: [],
    status: 'Unpublished',
  };
};

export default AvailabilityPeriodWizard;
