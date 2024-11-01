import { InsetText } from '@components/nhsuk-frontend';
import { TimeComponents } from '@types';

type CapacityCalculationProps = {
  slotLength: number;
  capacity: number;
  startTime: TimeComponents;
  endTime: TimeComponents;
};

const CapacityCalculation = (props: CapacityCalculationProps) => {
  return <InsetText>{calculateCapacity(props)}</InsetText>;
};

const calculateCapacity = ({
  slotLength,
  capacity,
  startTime,
  endTime,
}: CapacityCalculationProps): string => {
  if (
    !Number.isInteger(slotLength) ||
    !Number.isInteger(capacity) ||
    !Number.isInteger(startTime.hour) ||
    !Number.isInteger(startTime.minute) ||
    !Number.isInteger(endTime.hour) ||
    !Number.isInteger(endTime.minute)
  ) {
    return 'No capacity.';
  }

  const startMinutes = startTime.hour * 60 + startTime.minute;
  const endMinutes = endTime.hour * 60 + endTime.minute;
  const totalMinutesAvailable = endMinutes - startMinutes;

  const totalSlots = Math.floor(totalMinutesAvailable / slotLength);
  const slotsPerHour = Math.floor(60 / slotLength);

  const appointmentsPerHour = slotsPerHour * capacity;
  const appointmentsPerDay = totalSlots * capacity;

  if (
    Number.isNaN(appointmentsPerHour) ||
    Number.isNaN(appointmentsPerDay) ||
    !Number.isInteger(appointmentsPerHour) ||
    !Number.isInteger(appointmentsPerDay) ||
    appointmentsPerDay < 1 ||
    appointmentsPerHour < 1
  ) {
    return 'No capacity.';
  }

  return `${appointmentsPerDay} appointments per day. ${appointmentsPerHour} per hour.`;
};

export { calculateCapacity };
export default CapacityCalculation;
