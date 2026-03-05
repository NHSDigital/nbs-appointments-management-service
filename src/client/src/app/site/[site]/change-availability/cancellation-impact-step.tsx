'use client';
import { BackLink, Button, ButtonGroup } from '@components/nhsuk-frontend';
import { Heading } from 'nhsuk-react-components';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';
import { useFormContext } from 'react-hook-form';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import { ProposeCancelDateRangeResponse } from '@types';

interface Props {
  cancelADateRangeWithBookingsEnabled: boolean;
  site: string;
}

const CancellationImpactStep = ({
  cancelADateRangeWithBookingsEnabled,
  site,
  goToPreviousStep,
  goToNextStep,
}: InjectedWizardProps & Props) => {
  const router = useRouter();

  const { watch } = useFormContext<ChangeAvailabilityFormValues>();
  const proposedSummary = watch(
    'proposedCancellationSummary',
  ) as ProposeCancelDateRangeResponse;

  const renderCannotCancel = () => (
    <>
      <Heading headingLevel="h2">You cannot cancel these sessions</Heading>
      <p>
        There are existing bookings for these sessions. You should first cancel
        the bookings, then return to change availability.
      </p>
      <ButtonGroup>
        <Button
          onClick={() => {
            router.push(`/site/${site}/view-availability`);
          }}
        >
          Return to view availability
        </Button>
        <Button styleType="secondary" onClick={goToPreviousStep}>
          Select different dates
        </Button>
      </ButtonGroup>
    </>
  );

  const renderNoSessions = () => (
    <>
      <Heading headingLevel="h2">
        There are no sessions in this date range
      </Heading>
      <p>You should choose a new date range.</p>
      <Button onClick={goToPreviousStep}>Choose a new date range.</Button>
    </>
  );
  const renderNoBookings = () => (
    <>
      <Heading headingLevel="h2">
        {`You are about to cancel ${proposedSummary.sessionCount} ${proposedSummary.sessionCount > 1 ? 'sessions' : 'session'}`}
      </Heading>
      <p>
        There are no bookings for{' '}
        {proposedSummary.sessionCount > 1 ? 'these sessions' : 'this session'}
      </p>
      <Button onClick={goToNextStep}>Continue</Button>
    </>
  );

  const renderContent = () => {
    if (!cancelADateRangeWithBookingsEnabled) {
      return renderCannotCancel();
    }

    if (proposedSummary.sessionCount === 0) {
      return renderNoSessions();
    }

    if (proposedSummary.bookingCount === 0) {
      return renderNoBookings();
    }
  };

  return (
    <>
      <BackLink
        onClick={() => goToPreviousStep()}
        renderingStrategy="client"
        text="Back"
      />
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">{renderContent()}</div>
      </div>
    </>
  );
};

export default CancellationImpactStep;
