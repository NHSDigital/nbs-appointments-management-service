'use client';

import { Site } from '@types';
import React from 'react';
import WeekView from './week-view';
import { useAvailability } from '@hooks/useAvailability';
import {
  formatDateForUrl,
  getDaysOfTheWeek,
  parseDate,
} from '@services/timeService';
import { Pagination } from '@components/nhsuk-frontend';

type WeekViewProps = {
  referenceDate: string;
  site: Site;
};

const WeekPage = ({ referenceDate, site }: WeekViewProps) => {
  const parsedDate = parseDate(referenceDate);

  const { blocks, copyDay, pasteDay, copyWeek, pasteWeek } = useAvailability();

  const lastWeek = parsedDate.subtract(1, 'week');
  const nextWeek = parsedDate.add(1, 'week');

  return (
    <>
      <Pagination
        previous={{
          title: `wk/c ${lastWeek.format('MMMM DD')}`,
          href: `/site/${site.id}/availability/week?date=${formatDateForUrl(lastWeek)}`,
        }}
        next={{
          title: `wk/c ${nextWeek.format('MMMM DD')}`,
          href: `/site/${site.id}/availability/week?date=${formatDateForUrl(lastWeek)}`,
        }}
      />
      <WeekView
        blocks={blocks}
        week={getDaysOfTheWeek(parsedDate)}
        copyDay={copyDay}
        pasteDay={pasteDay}
        copyWeek={copyWeek}
        pasteWeek={pasteWeek}
      />
    </>
  );
};

export default WeekPage;
