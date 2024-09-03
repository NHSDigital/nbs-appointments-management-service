import { AvailabilityBlock, WeekInfo } from '@types';
import { timeSort } from './common';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs, { Dayjs } from 'dayjs';
import { DaySummary } from '../day-summary';
import NhsPaging from '../pager';
import { usePathname } from 'next/navigation';
import { When } from '@components/when';
import React from 'react';

type WeekViewProps = {
  onAddBlock: (block: AvailabilityBlock) => void;
  week: WeekInfo;
  blocks: AvailabilityBlock[];
  copyDay: (day: Dayjs) => void;
  pasteDay: (day: Dayjs) => void;
  copyWeek: (day: Dayjs) => void;
  pasteWeek: (day: Dayjs) => void;
};

const WeekView = ({
  onAddBlock,
  week,
  blocks,
  copyDay,
  pasteDay,
  copyWeek,
  pasteWeek,
}: WeekViewProps) => {
  const [copyWeekText, setCopyWeekText] = React.useState('Copy week');
  const pathname = usePathname();

  const wrappedCopy = () => {
    copyWeek(week.commencing);
    setCopyWeekText('Week copied');
    setTimeout(() => setCopyWeekText('Copy week'), 1000);
  };

  const days = [];
  for (let i = 0; i < 7; i++) days.push(week.commencing.add(i, 'day'));

  const weekUrl = (i: number) => {
    const params = new URLSearchParams();

    params.set('wn', (week.weekNumber + i).toString());
    return `${pathname}?${params.toString()}`;
  };

  return (
    <>
      <div style={{ display: 'flex', alignItems: 'baseline' }}>
        <h2>
          Availability for Week Commencing {week.commencing.format('MMMM DD')}
        </h2>
        <a href="#" onClick={wrappedCopy} style={{ marginLeft: '24px' }}>
          {copyWeekText}
        </a>
        <a
          href="#"
          onClick={() => pasteWeek(week.commencing)}
          style={{ marginLeft: '24px' }}
        >
          Paste week
        </a>
      </div>
      <NhsPaging
        nextLink={weekUrl(1)}
        nextText="Next week"
        prevLink={weekUrl(-1)}
        prevText="Previous week"
      />
      <div className="nhsuk-width-container-fluid">
        <ul
          className="nhsuk-grid-row nhsuk-card-group"
          style={{ padding: '20px' }}
        >
          {days.map(d => (
            <li
              key={d.format('DD ddd')}
              className="nhsuk-grid-column-one-third nhsuk-card-group__item"
            >
              <DayCard
                day={d}
                action={onAddBlock}
                blocks={blocks}
                copyDay={copyDay}
                pasteDay={pasteDay}
              />
            </li>
          ))}
        </ul>
      </div>
    </>
  );
};

type DayCardProps = {
  day: dayjs.Dayjs;
  action: (block: AvailabilityBlock) => void;
  blocks: AvailabilityBlock[];
  copyDay: (day: Dayjs) => void;
  pasteDay: (day: Dayjs) => void;
};

const DayCard = ({ day, blocks, action, copyDay, pasteDay }: DayCardProps) => {
  const [copyLinkText, setCopyLinkText] = React.useState('Copy day');

  const copyDayWrapper = () => {
    copyDay(day);
    setCopyLinkText('Day copied');
    setTimeout(() => setCopyLinkText('Copy day'), 1000);
  };

  const canEdit = dayjs().isBefore(day);

  const filteredBlocks = React.useMemo(
    () => blocks.filter(b => b.day.isSame(day)).toSorted(timeSort),
    [blocks, day],
  );

  const defaultBlock = {
    day,
    start: '09:00',
    end: '12:00',
    appointmentLength: 5,
    sessionHolders: 1,
    services: [],
    isPreview: true,
  };

  return (
    <div className="nhsuk-card nhsuk-card">
      <div className="nhsuk-card__content nhsuk-card__content--primary">
        <h2 className="nhsuk-card__heading nhsuk-heading-m">
          {day.format('DD ddd')}
        </h2>
        <DaySummary
          blocks={filteredBlocks}
          hasError={() => false}
          showBreaks={false}
        />
        <When condition={canEdit}>
          <a href="#" onClick={() => action(defaultBlock)}>
            Edit
          </a>
          <a href="#" onClick={copyDayWrapper} style={{ marginLeft: '16px' }}>
            {copyLinkText}
          </a>
          <a
            href="#"
            onClick={() => pasteDay(day)}
            style={{ marginLeft: '16px' }}
          >
            Paste Day
          </a>
        </When>
      </div>
    </div>
  );
};

export default WeekView;
