/* eslint-disable import/no-extraneous-dependencies */
'use client';

import { When } from '@components/when';
import { AvailabilityBlock, WeekInfo } from '@types';
import dayjs from 'dayjs';
import { useSearchParams } from 'next/navigation';
import React from 'react';
import { timeSort } from './common';
import SessionView from './session-view';
import WeekView from './week-view';

const AvailabilityPage = () => {
  const [mode, setMode] = React.useState('week');
  const [blocks, setBlocks] = React.useState([] as AvailabilityBlock[]);
  const [selectedDay, setSelectedDay] = React.useState<dayjs.Dayjs>();
  const searchParams = useSearchParams();
  const weekNumber = parseInt(searchParams.get('wn')!);

  const weekStart = dayjs('2024-01-01').add(weekNumber - 1, 'week');

  const weekInfo: WeekInfo = {
    weekNumber: weekNumber,
    month: weekStart.format('MMMM'),
    commencing: weekStart,
  };

  const onAddBlock = (day: dayjs.Dayjs) => {
    setSelectedDay(day);
    setMode('session');
  };

  const saveTimeBlock = (block: AvailabilityBlock) => {
    setBlocks([...blocks, block]);
    setMode('week');
  };

  const dayBlocks = (d: dayjs.Dayjs) =>
    blocks.filter(b => b.day.isSame(d)).sort(timeSort);

  React.useEffect(() => {
    const storedAvailability = localStorage.getItem('availability');
    if (storedAvailability) {
      const temp = JSON.parse(storedAvailability) as AvailabilityBlock[];
      const loaded = temp.map(t => ({
        ...t,
        day: dayjs(t.day),
      })) as AvailabilityBlock[];
      setBlocks(loaded);
    }
  }, []);

  React.useEffect(() => {
    if (blocks.length > 0)
      localStorage.setItem('availability', JSON.stringify(blocks));
  }, [blocks]);

  return (
    <div className="nhsuk-width-container-fluid">
      <When condition={mode === 'week'}>
        <WeekView onAddBlock={onAddBlock} blocks={blocks} week={weekInfo} />
      </When>
      <When condition={mode === 'session'}>
        <SessionView
          day={selectedDay!}
          blocks={dayBlocks(selectedDay!)}
          onSave={saveTimeBlock}
          onCancel={() => setMode('week')}
        />
      </When>
    </div>
  );
};

export default AvailabilityPage;
