'use client';

import { AvailabilityBlock, WeekInfo } from '@types';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';
import WeekView from './week-view';
import { useAvailability } from '../blocks';

const WeekPage = () => {
  const { blocks, copyDay, pasteDay, copyWeek, pasteWeek } = useAvailability();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();
  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  const weekNumber = parseInt(searchParams.get('wn')!);

  const weekStart = dayjs('2024-01-01').add(weekNumber - 1, 'week');

  const weekInfo: WeekInfo = {
    weekNumber: weekNumber,
    month: weekStart.format('MMMM'),
    commencing: weekStart,
  };

  const editSessionUrl = (block: AvailabilityBlock) => {
    const params = new URLSearchParams(searchParams);
    params.delete('wn');
    params.set('date', block.day.format('YYYY-MM-DD'));
    if (!block.isPreview) params.set('block', block.start);
    replace(`${pathname}/session?${params.toString()}`);
  };

  return (
    <WeekView
      onAddBlock={editSessionUrl}
      blocks={blocks}
      week={weekInfo}
      copyDay={copyDay}
      pasteDay={pasteDay}
      copyWeek={copyWeek}
      pasteWeek={pasteWeek}
    />
  );
};

export default WeekPage;
