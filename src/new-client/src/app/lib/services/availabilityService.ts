const services = [
  {
    key: 'COVID:5_11_10',
    value: 'Covid 5-11',
  },
  {
    key: 'COVID:12_15',
    value: 'Covid 12-15',
  },
  {
    key: 'COVID:16_17',
    value: 'Covid 16-17',
  },
  {
    key: 'COVID:18_74',
    value: 'Covid 18-74',
  },
  {
    key: 'COVID:75',
    value: 'Covid 75+',
  },
  {
    key: 'FLU:18_64',
    value: 'Flu 18-64',
  },
  {
    key: 'FLU:65',
    value: 'Flu 65+',
  },
  {
    key: 'COVID_FLU:18_64',
    value: 'Flu and Covid 18-64',
  },
  {
    key: 'COVID_FLU:65_74',
    value: 'Flu and Covid 65-74',
  },
  {
    key: 'COVID_FLU:75',
    value: 'Flu and Covid 75+',
  },
];

const covidServices = [
  { id: 'COVID:5_11_10', displayName: 'Covid 5-11' },
  { id: 'COVID:12_15', displayName: 'Covid 12-15' },
  { id: 'COVID:16_17', displayName: 'Covid 16-17' },
  { id: 'COVID:18_74', displayName: 'Covid 18-74' },
  { id: 'COVID:75', displayName: 'Covid 75+' },
];

const fluServices = [
  { id: 'FLU:18_64', displayName: 'Flu 18-64' },
  { id: 'FLU:65', displayName: 'Flu 65+' },
];

const shinglesServices = [
  { id: 'SHINGLES:5_11_10', displayName: 'Shingles 5-11' },
  { id: 'SHINGLES:12_15', displayName: 'Shingles 12-15' },
  { id: 'SHINGLES:16_17', displayName: 'Shingles 16-17' },
  { id: 'SHINGLES:18_74', displayName: 'Shingles 18-74' },
  { id: 'SHINGLES:75', displayName: 'Shingles 75+' },
];

const pneumoniaServices = [
  { id: 'PNEUMONIA:5_11_10', displayName: 'Pneumonia 5-11' },
  { id: 'PNEUMONIA:12_15', displayName: 'Pneumonia 12-15' },
  { id: 'PNEUMONIA:16_17', displayName: 'Pneumonia 16-17' },
  { id: 'PNEUMONIA:18_74', displayName: 'Pneumonia 18-74' },
  { id: 'PNEUMONIA:75', displayName: 'Pneumonia 75+' },
];

const rsvServices = [
  { id: 'RSV:5_11_10', displayName: 'RSV 5-11' },
  { id: 'RSV:12_15', displayName: 'RSV 12-15' },
  { id: 'RSV:16_17', displayName: 'RSV 16-17' },
  { id: 'RSV:18_74', displayName: 'RSV 18-74' },
  { id: 'RSV:75', displayName: 'RSV 75+' },
];

const summariseServices = (
  selectedServices: string[],
  emptyMessage?: string,
) => {
  if (selectedServices.length === 0) return emptyMessage ?? 'Break period';
  let serviceString = '',
    latestEnd: string,
    latestType: string;
  const names = services
    .filter(svc => selectedServices.includes(svc.key))
    .map(svc => svc.value);
  names
    ?.sort((a, b) => {
      const regex = /(^.*?(?=\d))(\d*)/;
      /* eslint-disable @typescript-eslint/no-unused-vars */
      // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
      const [_a, aType, aLowerRange] = a.match(regex)!;
      // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
      const [_b, bType, bLowerRange] = b.match(regex)!;
      /* eslint-enable @typescript-eslint/no-unused-vars */
      if (aType > bType) return 1;
      if (aType < bType) return -1;
      return +aLowerRange < +bLowerRange ? -1 : 1;
    })
    .forEach(dN => {
      // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
      const [fullMatch, type, start, end] = dN.match(
        /(^.*?(?=\d))(\d*)[-]?(\d*)?/,
      )!;
      if (latestType === type) {
        if (+latestEnd + 1 === +start) {
          const endOrPlus = end ? end : start + '+';
          serviceString = serviceString.replace(
            new RegExp(latestEnd + '$'),
            endOrPlus,
          );
        } else {
          serviceString += end ? `, ${start}-${end}` : `, ${start}+`;
        }
      } else {
        serviceString += `${latestType ? ' | ' : ''}${fullMatch}${!end ? '+' : ''}`;
      }
      latestEnd = end;
      latestType = type;
    });
  return serviceString;
};

const days = [
  { key: 'mon', value: 'Monday' },
  { key: 'tue', value: 'Tuesday' },
  { key: 'wed', value: 'Wednesday' },
  { key: 'thu', value: 'Thursday' },
  { key: 'fri', value: 'Friday' },
  { key: 'sat', value: 'Saturday' },
  { key: 'sun', value: 'Sunday' },
];

const summariseDays = (opts: string[]) => {
  if (opts.length === 7) return 'Every day';

  if (opts.length > 0) {
    const summary = opts
      .map(opt => days.find(d => d.key === opt)?.value)
      .join();

    if (summary === 'Monday,Tuesday,Wednesday,Thursday,Friday')
      return 'Every week day';

    if (summary === 'Saturday,Sunday') return 'Weekends';

    return summary;
  }
};

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
  summariseServices,
  summariseDays,
  services,
  covidServices,
  fluServices,
  shinglesServices,
  pneumoniaServices,
  rsvServices,
  days,
};
