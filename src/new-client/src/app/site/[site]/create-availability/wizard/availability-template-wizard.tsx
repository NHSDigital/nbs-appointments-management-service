'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import StartAndEndDateStep from './start-and-end-date-step';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { Site } from '@types';
import SingleOrRepeatingSessionStep from './single-or-repeating-session-step';
import SummaryStep from './summary-step';
import saveAvailabilityTemplate from './save-availability-template';

export type AvailabilityTemplateFormValues = {
  startDateDay: number;
  startDateMonth: number;
  startDateYear: number;
  endDateDay: number;
  endDateMonth: number;
  endDateYear: number;
};

type Props = {
  site: Site;
};

const AvailabilityTemplateWizard = ({ site }: Props) => {
  const methods = useForm<AvailabilityTemplateFormValues>();

  const submitForm: SubmitHandler<AvailabilityTemplateFormValues> = async (
    form: AvailabilityTemplateFormValues,
  ) => {
    await saveAvailabilityTemplate(form, site);
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
            {stepProps => <StartAndEndDateStep {...stepProps} />}
          </WizardStep>
          <WizardStep>
            {stepProps => <SingleOrRepeatingSessionStep {...stepProps} />}
          </WizardStep>
          <WizardStep>{stepProps => <SummaryStep {...stepProps} />}</WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default AvailabilityTemplateWizard;
