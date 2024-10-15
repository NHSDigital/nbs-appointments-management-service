'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import StartAndEndDateStep from './start-and-end-date-step';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { AvailabilityPeriod, Site } from '@types';
import saveAvailabilityPeriod from './save-availability-period';
import SingleOrRepeatingSessionStep from './single-or-repeating-session-step';
import dayjs from 'dayjs';

export type AvailabilityPeriodFormValues = {
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

const AvailabilityPeriodWizard = ({ site }: Props) => {
  const methods = useForm<AvailabilityPeriodFormValues>();

  const submitForm: SubmitHandler<AvailabilityPeriodFormValues> = (
    form: AvailabilityPeriodFormValues,
  ) => {
    const availabilityPeriod = mapFormValuesToAvailabilityPeriod(form);
    saveAvailabilityPeriod(availabilityPeriod);
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
    startDate: new dayjs.Dayjs(
      new Date(
        formValues.startDateYear,
        formValues.startDateMonth - 1,
        formValues.startDateDay,
      ),
    ),
    endDate: new dayjs.Dayjs(
      new Date(
        formValues.endDateYear,
        formValues.endDateMonth,
        formValues.endDateDay,
      ),
    ),
    services: [],
    status: 'Unpublished',
  };
};

export default AvailabilityPeriodWizard;
