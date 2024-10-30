import { InsetText } from '@components/nhsuk-frontend';
import { TimeComponents } from '@types';

type CapacityCalculationProps = {
  slotLength: number;
  capacity: number;
  startTime: TimeComponents;
  endTime: TimeComponents;
};

const CapacityCalculation = ({
  slotLength,
  capacity,
  startTime,
  endTime,
}: CapacityCalculationProps) => {
  // TODO: Neaten this up a bit, test more edge cases etc.
  const startMinutes = startTime.hour * 60 + startTime.minute;
  const endMinutes = endTime.hour * 60 + endTime.minute;
  const totalMinutesAvailable = endMinutes - startMinutes;

  if (
    totalMinutesAvailable <= 0 ||
    Number.isNaN(capacity) ||
    Number.isNaN(slotLength)
  ) {
    return <InsetText>No capacity</InsetText>;
  }

  const totalSlots = Math.floor(totalMinutesAvailable / slotLength);
  const slotsPerHour = Math.floor(60 / slotLength);

  const appointmentsPerHour = slotsPerHour * capacity;
  const appointmentsPerDay = totalSlots * capacity;

  if (
    Number.isNaN(appointmentsPerHour) ||
    Number.isNaN(appointmentsPerDay) ||
    !Number.isInteger(appointmentsPerHour) ||
    !Number.isInteger(appointmentsPerDay)
  ) {
    return <InsetText>No capacity</InsetText>;
  }

  return (
    <InsetText>
      {`${appointmentsPerDay} appointments per day. ${appointmentsPerHour} per
      hour.`}
    </InsetText>
  );
};

export default CapacityCalculation;
