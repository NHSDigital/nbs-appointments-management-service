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
  dateTimeStringFormat,
  dateStringFormat,
  parseDateStringToUkDatetime,
  parseDateToUkDatetime,
} from '@services/timeService';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';

type CancelFormValue = {
  cancelAppointment: 'yes' | 'no';
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
  const { replace } = useRouter();
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful },
  } = useForm<CancelFormValue>({
    defaultValues: {
      cancelAppointment: 'yes',
    },
  });
  const summaryItems = mapSummaryData(booking, clinicalServices);

  const submitForm: SubmitHandler<CancelFormValue> = async (
    form: CancelFormValue,
  ) => {
    if (form.cancelAppointment === 'yes') {
      await cancelAppointment(booking.reference, site);
    }

    const returnDate = parseDateStringToUkDatetime(booking.from).format(
      dateStringFormat,
    );

    replace(
      `/site/${site}/view-availability/daily-appointments?date=${returnDate}&tab=1&page=1`,
    );
  };

  return (
    <>
      {summaryItems && <SummaryList {...summaryItems} />}
      <form onSubmit={handleSubmit(submitForm)}>
        <FormGroup>
          <RadioGroup>
            <Radio
              label="Yes, I want to cancel this appointment"
              id="cancelOperation-yes"
              value="yes"
              {...register('cancelAppointment')}
            />
            <Radio
              label="No, I do not want to cancel this appointment"
              id="cancelOperation-no"
              value="no"
              {...register('cancelAppointment')}
            />
          </RadioGroup>
        </FormGroup>

        {isSubmitting || isSubmitSuccessful ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit">Continue</Button>
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

  const bookingDate = parseDateStringToUkDatetime(
    booking.from,
    dateTimeStringFormat,
  );
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
    value: parseDateToUkDatetime(booking.attendeeDetails.dateOfBirth).format(
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
