import { AvailabilityBlock } from '@types';
import dayjs from 'dayjs';

const timeSort = function (a: AvailabilityBlock, b: AvailabilityBlock) {
  return (
    Date.parse('1970/01/01 ' + a.start) - Date.parse('1970/01/01 ' + b.start)
  );
};

const isBefore = (lhs: string, rhs: string) => {
  const a = parseInt(lhs);
  const b = parseInt(rhs);
  return a < b;
};

const timeAsInt = (time: string) => parseInt(time.replace(':', ''));

const isWithin = (
  a: { start: string; end: string },
  b: { start: string; end: string },
) => {
  const boundsOfA = { start: timeAsInt(a.start), end: timeAsInt(a.end) };
  const boundsOfB = { start: timeAsInt(b.start), end: timeAsInt(b.end) };

  return boundsOfA.start >= boundsOfB.start && boundsOfA.end <= boundsOfB.end;
};

const conflictsWith = (
  a: { start: string; end: string },
  b: { start: string; end: string },
) => {
  const boundsOfA = { start: timeAsInt(a.start), end: timeAsInt(a.end) };
  const boundsOfB = { start: timeAsInt(b.start), end: timeAsInt(b.end) };

  if (boundsOfA.start >= boundsOfB.start && boundsOfA.start < boundsOfB.end)
    return true;
  if (boundsOfB.start >= boundsOfA.start && boundsOfB.start < boundsOfA.end)
    return true;
  return false;
};

const calculateNumberOfAppointments = (
  block: AvailabilityBlock,
  blocks: AvailabilityBlock[],
): number => {
  const startDateTime = block.day.format('YYYY-MM-DD ') + block.start;
  const start = dayjs(startDateTime);

  const endDateTime = block.day.format('YYYY-MM-DD ') + block.end;
  const end = dayjs(endDateTime);
  const minutes = end.diff(start, 'minute');
  const unadjusted = (minutes / block.appointmentLength) * block.sessionHolders;

  if (!block.isBreak) {
    const breaks = blocks
      .filter(b => b.isBreak && isWithin(b, block))
      .map(b => {
        return calculateNumberOfAppointments(
          {
            ...b,
            sessionHolders: block.sessionHolders,
            appointmentLength: block.appointmentLength,
          },
          blocks,
        );
      });

    const reduction = breaks?.reduce((a, b) => a + b, 0) ?? 0;
    return unadjusted - reduction;
  }

  return unadjusted;
};

export {
  timeSort,
  isBefore,
  conflictsWith,
  timeAsInt,
  isWithin,
  calculateNumberOfAppointments,
};
