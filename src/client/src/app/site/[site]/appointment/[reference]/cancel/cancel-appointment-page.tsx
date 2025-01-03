'use client';
import {
  Button,
  FormGroup,
  Radio,
  RadioGroup,
  Spinner,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { cancelAppointment } from '@services/appointmentsService';
import { Booking, clinicalServices } from '@types';
import dayjs from 'dayjs';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';

type CancelFormValue = {
  cancelAppointment: 'yes' | 'no';
};

const CancelAppointmentPage = ({
  booking,
  site,
}: {
  booking: Booking;
  site: string;
}) => {
  const { replace } = useRouter();
  const {
    register,
    reset,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful },
  } = useForm<CancelFormValue>({
    defaultValues: {
      cancelAppointment: 'yes',
    },
  });
  const summaryItems = mapSummaryData(booking);

  const cancelOperation = { ...register('cancelAppointment') };

  const submitForm: SubmitHandler<CancelFormValue> = async (
    form: CancelFormValue,
  ) => {
    if (form.cancelAppointment === 'yes') {
      await cancelAppointment(booking.reference, site);
    }

    const returnDate = dayjs(booking.from).format('YYYY-MM-DD');

    replace(
      `/site/${site}/view-availability/daily-appointments?date=${returnDate}&page=1`,
    );
  };

  return (
    <>
      {summaryItems && <SummaryList {...summaryItems} />}
      <form onSubmit={handleSubmit(submitForm)} noValidate={true}>
        <FormGroup>
          <RadioGroup>
            <Radio
              label="Yes, I want to cancel this appointment"
              {...{
                ...cancelOperation,
                onChange: e => {
                  reset({
                    cancelAppointment: 'yes',
                  });
                  cancelOperation.onChange(e);
                },
              }}
              id="cancelOperation-yes"
              value="yes"
            />
            <Radio
              label="No, I do not want to cancel this appointment"
              {...{
                ...cancelOperation,
                onChange: e => {
                  reset({
                    cancelAppointment: 'no',
                  });
                  cancelOperation.onChange(e);
                },
              }}
              id="cancelOperation-no"
              value="no"
            />
          </RadioGroup>
        </FormGroup>

        {isSubmitting || isSubmitSuccessful ? (
          <Spinner />
        ) : (
          <Button type="submit">Continue</Button>
        )}
      </form>
    </>
  );
};

export default CancelAppointmentPage;

const mapSummaryData = (booking: Booking) => {
  if (!booking) {
    return undefined;
  }

  const items: SummaryListItem[] = [];

  const bookingDate = dayjs(booking.from);
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
  items.push({ title: 'NHS number', value: booking.attendeeDetails.nhsNumber });
  items.push({
    title: 'Date of birth',
    value: dayjs(booking.attendeeDetails.dateOfBirth).format('D MMMM YYYY'),
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
