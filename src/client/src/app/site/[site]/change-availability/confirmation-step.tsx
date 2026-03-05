'use client';
import { InjectedWizardProps } from '@components/wizard';
import Link from 'next/link';
import { useFormContext } from 'react-hook-form';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import { Heading } from 'nhsuk-react-components';

interface Props {
  site: string;
}

const ConfirmationStep = ({ site }: InjectedWizardProps & Props) => {
  const { getValues } = useFormContext<ChangeAvailabilityFormValues>();
  const { cancellationSummary } = getValues();

  if (!cancellationSummary)
    throw new Error("Couldn't load cancellation summary");

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-two-thirds">
        <Heading headingLevel="h2">
          {`${cancellationSummary.cancelledSessionsCount} ${
            cancellationSummary.cancelledSessionsCount > 1
              ? 'sessions'
              : 'session'
          } cancelled`}
        </Heading>

        <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
          Next steps
        </legend>
        <br />
        <ul>
          <Link href={`/site/${site}/create-availability/wizard`}>
            <li>Create availability</li>
          </Link>
          <Link href={`/site/${site}/view-availability`}>
            <li>Go back to view availability</li>
          </Link>
        </ul>
      </div>
    </div>
  );
};

export default ConfirmationStep;
