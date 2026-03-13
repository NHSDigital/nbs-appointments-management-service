'use client';
import {
  BackLink,
  Button,
  ButtonGroup,
  FormGroup,
  RadioGroup,
  Radio,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';
import { useFormContext } from 'react-hook-form';
import { ChangeAvailabilityFormValues } from './change-availability-form-schema';
import { Heading, ErrorSummary } from 'nhsuk-react-components';

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

  const {
    getValues,
    register,
    trigger,
    formState: { errors },
  } = useFormContext<ChangeAvailabilityFormValues>();
  const { proposedCancellationSummary } = getValues();

  if (!proposedCancellationSummary)
    throw new Error("Couldn't load cancellation summary");
  const cancellationDecision = { ...register('cancellationDecision') };

  const customErrorMessage = errors.cancellationDecision
    ? `Select what you want to do with the ${(proposedCancellationSummary?.bookingCount ?? 0) > 1 ? 'bookings' : 'booking'}`
    : undefined;

  const onResolveBookingsContinue = async () => {
    const isValid = await trigger('cancellationDecision');

    if (!isValid) return;

    goToNextStep();
  };

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
        {`You are about to cancel ${proposedCancellationSummary.sessionCount} ${proposedCancellationSummary.sessionCount > 1 ? 'sessions' : 'session'}`}
      </Heading>
      <p>
        There are no bookings for{' '}
        {proposedCancellationSummary.sessionCount > 1
          ? 'these sessions'
          : 'this session'}
      </p>
      <Button onClick={goToNextStep}>Continue</Button>
    </>
  );

  const renderResolveBookings = () => (
    <>
      {customErrorMessage && (
        <ErrorSummary>
          <ErrorSummary.Title>There is a problem</ErrorSummary.Title>
          <ErrorSummary.List>
            <ErrorSummary.ListItem href="#cancellation-keep-bookings">
              {customErrorMessage}
            </ErrorSummary.ListItem>
          </ErrorSummary.List>
        </ErrorSummary>
      )}
      <Heading headingLevel="h2">
        You are about to cancel {proposedCancellationSummary.sessionCount}{' '}
        {proposedCancellationSummary.sessionCount > 1 ? 'sessions' : 'session'}
      </Heading>

      <p>
        There{' '}
        {proposedCancellationSummary.bookingCount == 1
          ? `is ${proposedCancellationSummary.bookingCount} booking`
          : `are ${proposedCancellationSummary.bookingCount} bookings`}{' '}
        for these sessions
      </p>

      <Heading headingLevel="h4">
        What do you want to do with the{' '}
        {proposedCancellationSummary.bookingCount}{' '}
        {proposedCancellationSummary.bookingCount > 1 ? 'bookings' : 'booking'}?
      </Heading>

      <FormGroup error={customErrorMessage}>
        <RadioGroup>
          <Radio
            label="Keep bookings"
            hint="These will stay in your appointments list"
            {...{
              ...cancellationDecision,
              onChange: e => {
                cancellationDecision.onChange(e);
              },
            }}
            id="cancellation-keep-bookings"
            value="keep-bookings"
          />
          <Radio
            label="Cancel bookings"
            hint="We will email or text people to confirm the cancellation"
            {...{
              ...cancellationDecision,
              onChange: e => {
                cancellationDecision.onChange(e);
              },
            }}
            id="cancellation-cancel-bookings"
            value="cancel-bookings"
          />
        </RadioGroup>
      </FormGroup>

      <ButtonGroup>
        <Button
          type="button"
          onClick={() => {
            onResolveBookingsContinue();
          }}
        >
          Continue
        </Button>
      </ButtonGroup>
    </>
  );

  const renderContent = () => {
    if (proposedCancellationSummary.sessionCount === 0) {
      return renderNoSessions();
    }

    if (proposedCancellationSummary.bookingCount === 0) {
      return renderNoBookings();
    }

    if (!cancelADateRangeWithBookingsEnabled) {
      return renderCannotCancel();
    }

    return renderResolveBookings();
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
