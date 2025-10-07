import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  FormGroup,
  Radio,
  RadioGroup,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';
import { useFormContext } from 'react-hook-form';
import { CancelDayFromValues } from '../cancel-day-wizard';
import CancelDaySummary from '../cancel-day-summary';
import { DaySummaryV2, ClinicalService, Site } from '@types';
import { DayJsType, RFC3339Format } from '@services/timeService';

export type CancelChoiceStepProps = {
  site: Site;
  date: DayJsType;
  daySummary: DaySummaryV2;
  clinicalServices: ClinicalService[];
};

const CancelChoiceStep = ({
  goToNextStep,
  goToPreviousStep,
  site,
  date,
  daySummary,
  clinicalServices,
}: InjectedWizardProps & CancelChoiceStepProps) => {
  const router = useRouter();
  const { formState, register, trigger, setValue, watch } =
    useFormContext<CancelDayFromValues>();
  const { errors } = formState;
  const cancelChoiceWatch = watch('cancelChoice');

  const onContinue = async () => {
    const stepIsValid = await trigger('cancelChoice', { shouldFocus: true });
    if (!stepIsValid) {
      return;
    }

    setValue('cancelChoice', cancelChoiceWatch);

    if (cancelChoiceWatch === 'false') {
      router.push(
        `/site/${site.id}/view-availability/week?date=${date.format(RFC3339Format)}`,
      );
      return;
    }

    goToNextStep();
  };

  return (
    <>
      <BackLink
        onClick={goToPreviousStep}
        renderingStrategy="client"
        text="Back"
      />
      <NhsHeading
        title={`Cancel ${date.format('dddd D MMMM')}`}
        caption={site.name}
      />

      <CancelDaySummary
        clinicalServices={clinicalServices}
        date={date.format('dddd D MMMM')}
        daySummary={daySummary}
      />

      <FormGroup
        legend="Are you sure you want to cancel this day?"
        error={errors.cancelChoice?.message}
      >
        <RadioGroup>
          <Radio
            label="Yes, I want to cancel the appointments"
            hint="Cancel day"
            id="yes-cancel"
            value="true"
            {...register('cancelChoice', {
              required: { value: true, message: 'Select an option' },
            })}
          />
          <Radio
            label="No, I don't want to cancel the appointments"
            hint="I want to keep my day, do not cancel"
            id="no"
            value="false"
            {...register('cancelChoice', {
              required: { value: true, message: 'Select an option' },
            })}
          />
        </RadioGroup>
      </FormGroup>

      <Button
        type="button"
        onClick={async () => {
          await onContinue();
        }}
      >
        Continue
      </Button>
    </>
  );
};

export default CancelChoiceStep;
