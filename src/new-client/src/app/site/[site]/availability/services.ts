const services = [
  {
    key: 'COVID:18_74',
    value: 'Covid 18-74',
  },
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

const serviceSummary = (selectedServices: string[]) => {
  if (selectedServices.length === 0) return 'Break period';
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

const days = [
  { key: 'mon', value: 'Monday' },
  { key: 'tue', value: 'Tuesday' },
  { key: 'wed', value: 'Wednesday' },
  { key: 'thu', value: 'Thursday' },
  { key: 'fri', value: 'Friday' },
  { key: 'sat', value: 'Saturday' },
  { key: 'sun', value: 'Sunday' },
];

export { services, serviceSummary, days, summariseDays };
