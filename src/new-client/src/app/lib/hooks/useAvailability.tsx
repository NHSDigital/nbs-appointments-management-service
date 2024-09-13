import { AvailabilityBlock } from '@types';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';
import React from 'react';

import isoWeek from 'dayjs/plugin/isoWeek';
import customParseFormat from 'dayjs/plugin/customParseFormat';

dayjs.extend(isoWeek);
dayjs.extend(customParseFormat);

export const useAvailability = () => {
  const [blocks, setBlocks] = React.useState<AvailabilityBlock[]>(
    [] as AvailabilityBlock[],
  );

  const dayBlocks = (day: dayjs.Dayjs) => blocks.filter(b => b.day.isSame(day));

  const copyWeek = (day: dayjs.Dayjs) => {
    const weekStart = day.startOf('isoWeek');
    localStorage.setItem(
      'clip_week',
      JSON.stringify(
        blocks.filter(
          av =>
            av.day.isAfter(weekStart.add(-1, 'day')) &&
            av.day.isBefore(weekStart.add(7, 'day')),
        ),
      ),
    );
  };

  const pasteWeek = (day: dayjs.Dayjs) => {
    const copiedBlocks = loadBlocks('clip_week');
    const movedBlocks = copiedBlocks.map(cb => ({
      day: day.add(cb.day.day() === 0 ? 6 : cb.day.day() - 1, 'day'),
      start: cb.start,
      end: cb.end,
      appointmentLength: cb.appointmentLength,
      sessionHolders: cb.sessionHolders,
      services: [...cb.services],
      isPreview: false,
      isBreak: cb.isBreak,
    }));
    setBlocks([
      ...blocks.filter(
        av => av.day.isBefore(day) || av.day.isAfter(day.add(6, 'day')),
      ),
      ...movedBlocks,
    ]);
  };

  const copyDay = (day: dayjs.Dayjs) => {
    localStorage.setItem(
      'clip_day',
      JSON.stringify(blocks.filter(b => b.day.isSame(day))),
    );
  };

  const pasteDay = (day: dayjs.Dayjs) => {
    const copiedBlocks = loadBlocks('clip_day');
    const movedDays: AvailabilityBlock[] = copiedBlocks.map(cb => ({
      day,
      start: cb.start,
      end: cb.end,
      appointmentLength: cb.appointmentLength,
      sessionHolders: cb.sessionHolders,
      services: cb.services,
      isPreview: false,
      isBreak: cb.isBreak,
    }));

    const modifiedBlocks = [
      ...blocks.filter(b => !b.day.isSame(day)),
      ...movedDays,
    ];
    setBlocks(modifiedBlocks);
  };

  const saveBlock = (
    block: AvailabilityBlock,
    oldBlock?: AvailabilityBlock,
  ) => {
    setBlocks([
      ...blocks.filter(
        b => !(b.day.isSame(block.day) && b.start === oldBlock?.start),
      ),
      block,
    ]);
  };

  const removeBlock = (block: AvailabilityBlock) => {
    setBlocks(
      blocks.filter(b => !(b.day.isSame(block.day) && b.start === block.start)),
    );
  };

  React.useEffect(() => {
    if (blocks.length > 0)
      localStorage.setItem('availability', JSON.stringify(blocks));
  }, [blocks]);

  const loadBlocks = (key: string) => {
    const availabilityJson = localStorage.getItem(key);
    if (availabilityJson) {
      const temp = JSON.parse(availabilityJson) as {
        day: string;
        start: string;
        end: string;
        appointmentLength: number;
        sessionHolders: number;
        services: string[];
        isBreak: boolean;
      }[];
      return temp.map(t => ({
        day: dayjs(t.day),
        start: t.start,
        end: t.end,
        appointmentLength: t.appointmentLength,
        sessionHolders: t.sessionHolders,
        services: t.services,
        isBreak: t.isBreak,
      }));
    } else return [];
  };

  React.useEffect(() => {
    setBlocks(loadBlocks('availability'));
  }, []);

  return {
    blocks,
    dayBlocks,
    saveBlock,
    removeBlock,
    copyDay,
    pasteDay,
    copyWeek,
    pasteWeek,
  };
};
