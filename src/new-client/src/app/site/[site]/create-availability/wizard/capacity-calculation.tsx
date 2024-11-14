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
  appointmentsPerHour?: number;
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
        {capacity.appointmentsPerHour !== undefined && (
          <>
            <br />
            Up to <strong>{capacity.appointmentsPerHour}</strong> appointments
            per hour
          </>
        )}
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
  const parsedSlotLength = Number(slotLength);
  const parsedCapacity = Number(capacity);
  const parsedStartTimeHour = Number(startTime.hour);
  const parsedStartTimeMinute = Number(startTime.minute);
  const parsedEndTimeHour = Number(endTime.hour);
  const parsedEndTimeMinute = Number(endTime.minute);

  if (
    !Number.isInteger(parsedSlotLength) ||
    !Number.isInteger(parsedCapacity) ||
    !Number.isInteger(parsedStartTimeHour) ||
    !Number.isInteger(parsedStartTimeMinute) ||
    !Number.isInteger(parsedEndTimeHour) ||
    !Number.isInteger(parsedEndTimeMinute)
  ) {
    return { appointmentsPerSession: 0, appointmentsPerHour: 0 };
  }

  const startMinutes = parsedStartTimeHour * 60 + parsedStartTimeMinute;
  const endMinutes = parsedEndTimeHour * 60 + parsedEndTimeMinute;
  const totalMinutesAvailable = endMinutes - startMinutes;

  const totalSlots = Math.floor(totalMinutesAvailable / parsedSlotLength);
  const slotsPerHour = Math.floor(60 / parsedSlotLength);

  const appointmentsPerHour = slotsPerHour * parsedCapacity;
  const appointmentsPerSession = totalSlots * parsedCapacity;

  if (
    Number.isNaN(appointmentsPerHour) ||
    Number.isNaN(appointmentsPerSession) ||
    !Number.isInteger(appointmentsPerHour) ||
    !Number.isInteger(appointmentsPerSession) ||
    appointmentsPerSession < 1 ||
    appointmentsPerHour < 1
  ) {
    return { appointmentsPerSession: 0, appointmentsPerHour: 0 };
  }

  return {
    appointmentsPerSession,
    appointmentsPerHour:
      totalMinutesAvailable >= 60 ? appointmentsPerHour : undefined,
  };
};

const sessionLengthInMinutes = (
  startTime: TimeComponents,
  endTime: TimeComponents,
): number => {
  const startMinutes = Number(startTime.hour) * 60 + Number(startTime.minute);
  const endMinutes = Number(endTime.hour) * 60 + Number(endTime.minute);

  return endMinutes - startMinutes;
};

export { calculateCapacity, sessionLengthInMinutes };
export default CapacityCalculation;
