import { AvailabilityBlock } from '@types';

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

const timeAsInt = (time: string) => time.replace(':', '');

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

export { timeSort, isBefore, conflictsWith };
