import { InsetText } from '@components/nhsuk-frontend';
import { TimeComponents } from '@types';

type CapacityCalculationProps = {
  slotLength: number;
  capacity: number;
  startTime: TimeComponents;
  endTime: TimeComponents;
};

type CapacityCalculationResult = {
  appointmentsPerSession: number;
  appointmentsPerHour: number;
};

const CapacityCalculation = (props: CapacityCalculationProps) => {
  const capacity = calculateCapacity(props);

  return (
    <InsetText>
      <strong>
        <p style={{ marginBottom: 0 }}>Capacity calculator</p>
      </strong>
      <p style={{ marginBottom: 0 }}>
        <strong>{capacity.appointmentsPerSession}</strong> total appointments in
        the session
        <br />
        <strong>{capacity.appointmentsPerHour}</strong> appointments per hour
        <br />
      </p>
    </InsetText>
  );
};

const calculateCapacity = ({
  slotLength,
  capacity,
  startTime,
  endTime,
}: CapacityCalculationProps): CapacityCalculationResult => {
  if (
    !Number.isInteger(slotLength) ||
    !Number.isInteger(capacity) ||
    !Number.isInteger(startTime.hour) ||
    !Number.isInteger(startTime.minute) ||
    !Number.isInteger(endTime.hour) ||
    !Number.isInteger(endTime.minute)
  ) {
    return { appointmentsPerSession: 0, appointmentsPerHour: 0 };
  }

  const startMinutes = startTime.hour * 60 + startTime.minute;
  const endMinutes = endTime.hour * 60 + endTime.minute;
  const totalMinutesAvailable = endMinutes - startMinutes;

  const totalSlots = Math.floor(totalMinutesAvailable / slotLength);
  const slotsPerHour = Math.floor(60 / slotLength);

  const appointmentsPerHour = slotsPerHour * capacity;
  const appointmentsPerSession = totalSlots * capacity;

  if (
    Number.isNaN(appointmentsPerHour) ||
    Number.isNaN(appointmentsPerSession) ||
    !Number.isInteger(appointmentsPerSession) ||
    !Number.isInteger(appointmentsPerSession) ||
    appointmentsPerSession < 1 ||
    appointmentsPerHour < 1
  ) {
    return { appointmentsPerSession: 0, appointmentsPerHour: 0 };
  }

  return {
    appointmentsPerSession,
    appointmentsPerHour,
  };
};

const sessionLengthInMinutes = (
  startTime: TimeComponents,
  endTime: TimeComponents,
): number => {
  const startMinutes = startTime.hour * 60 + startTime.minute;
  const endMinutes = endTime.hour * 60 + endTime.minute;

  return endMinutes - startMinutes;
};

export { calculateCapacity, sessionLengthInMinutes };
export default CapacityCalculation;
