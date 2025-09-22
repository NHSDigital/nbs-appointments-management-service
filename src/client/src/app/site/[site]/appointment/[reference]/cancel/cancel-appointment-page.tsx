'use client';
import {
  Button,
  FormGroup,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { cancelAppointment } from '@services/appointmentsService';
import { Booking, ClinicalService } from '@types';
import {
  dateTimeFormat,
  RFC3339Format,
  parseToUkDatetime,
} from '@services/timeService';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useTransition } from 'react';
import fromServer from '@server/fromServer';

type CancelFormValue = {
  cancellationReason?: 'CancelledByCitizen' | 'CancelledBySite';
};

const CancelAppointmentPage = ({
  booking,
  site,
  clinicalServices,
}: {
  booking: Booking;
  site: string;
  clinicalServices: ClinicalService[];
}) => {
  const [pendingSubmit, startTransition] = useTransition();
  const { replace } = useRouter();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CancelFormValue>({
    defaultValues: {},
  });
  const summaryItems = mapSummaryData(booking, clinicalServices);

  const submitForm: SubmitHandler<CancelFormValue> = async (
    form: CancelFormValue,
  ) => {
    startTransition(async () => {
      if (form.cancellationReason !== undefined) {
        await fromServer(
          cancelAppointment(booking.reference, site, form.cancellationReason),
        );

        const returnDate = parseToUkDatetime(booking.from).format(
          RFC3339Format,
        );

        replace(
          `/site/${site}/view-availability/daily-appointments?date=${returnDate}&tab=1&page=1`,
        );
      }
    });
  };

  return (
    <>
      {summaryItems && <SummaryList {...summaryItems} />}
      <form onSubmit={handleSubmit(submitForm)}>
        <FormGroup error={errors.cancellationReason?.message}>
          <legend className="nhsuk-fieldset__legend nhsuk-label--m">
            <h1 className="nhsuk-fieldset__heading">Select a reason</h1>
          </legend>
          <RadioGroup>
            <Radio
              label="Cancelled by the citizen"
              id="cancelOperation-citizen"
              value="CancelledByCitizen"
              {...register('cancellationReason', {
                required: 'Select a reason for cancelling the appointment',
              })}
            />
            <Radio
              label="Cancelled by the site"
              id="cancelOperation-site"
              value="CancelledBySite"
              {...register('cancellationReason', {
                required: 'Select a reason for cancelling the appointment',
              })}
            />
          </RadioGroup>
        </FormGroup>

        {pendingSubmit ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit">Cancel appointment</Button>
        )}
      </form>
    </>
  );
};

export default CancelAppointmentPage;

const mapSummaryData = (
  booking: Booking,
  clinicalServices: ClinicalService[],
) => {
  if (!booking) {
    return undefined;
  }

  const items: SummaryListItem[] = [];

  const bookingDate = parseToUkDatetime(booking.from, dateTimeFormat);
  const contactDetails =
    booking.contactDetails && booking.contactDetails.length > 0
      ? booking.contactDetails?.map(c => c.value)
      : 'Not provided';

  items.push({
    title: 'Date and time',
    value: [bookingDate.format('D MMMM YYYY'), bookingDate.format('H:mma')],
  });
  items.push({
    title: 'Name',
    value: `${booking.attendeeDetails.firstName} ${booking.attendeeDetails.lastName}`,
  });
  items.push({
    title: 'NHS number',
    value: booking.attendeeDetails.nhsNumber,
  });
  items.push({
    title: 'Date of birth',
    value: parseToUkDatetime(booking.attendeeDetails.dateOfBirth).format(
      'D MMMM YYYY',
    ),
  });
  items.push({ title: 'Contact information', value: contactDetails });
  items.push({
    title: 'Service',
    value:
      clinicalServices.find(c => c.value === booking.service)?.label ??
      booking.service,
  });

  const border = false;

  return { items, border };
};
