import { AvailabilityBlock } from '@types';
import dayjs from 'dayjs';
import React from 'react';
import { services } from './services';

export const useAvailability = () => {
  const [blocks, setBlocks] = React.useState<AvailabilityBlock[]>(
    [] as AvailabilityBlock[],
  );

  const dayBlocks = (day: dayjs.Dayjs) => blocks.filter(b => b.day.isSame(day));

  const saveBlock = (block: AvailabilityBlock) => {
    setBlocks([...blocks, block]);
  };

  React.useEffect(() => {
    if (blocks.length > 0)
      localStorage.setItem('availability', JSON.stringify(blocks));
  }, [blocks]);

  React.useEffect(() => {
    const availabilityJson = localStorage.getItem('availability');
    if (availabilityJson) {
      const temp = JSON.parse(availabilityJson) as {
        day: string;
        start: string;
        end: string;
        sessionHolders: number;
        services: string[];
      }[];
      setBlocks(
        temp.map(t => ({
          day: dayjs(t.day),
          start: t.start,
          end: t.end,
          sessionHolders: t.sessionHolders,
          services: t.services,
        })),
      );
    }
  }, []);

  return { blocks, dayBlocks };
};
