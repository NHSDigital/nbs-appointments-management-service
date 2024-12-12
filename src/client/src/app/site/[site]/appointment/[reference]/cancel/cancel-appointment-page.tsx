import {
  Button,
  Radio,
  RadioGroup,
  SummaryList,
  SummaryListItem,
} from '@components/nhsuk-frontend';
import { Booking, clinicalServices } from '@types';
import dayjs from 'dayjs';

type Props = {
  booking: Booking;
};

export const CancelAppointmentPage = ({ booking }: Props) => {
  const summaryItems = mapSummaryData(booking);

  return (
    <>
      {summaryItems && <SummaryList {...summaryItems} />}
      <RadioGroup>
        <Radio label="Yes, I want to cancel this appointment" />
        <Radio label="No, I do not want to cancel this appointment" />
      </RadioGroup>
      <br />
      <Button type="button">Continue</Button>
    </>
  );
};

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
