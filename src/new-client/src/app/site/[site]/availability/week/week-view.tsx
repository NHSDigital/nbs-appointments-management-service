import { AvailabilityBlock, Site } from '@types';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs, { Dayjs } from 'dayjs';
import { DaySummary } from '../../../../lib/components/day-summary';
import { When } from '@components/when';
import React from 'react';
import { timeSort } from '@services/availabilityService';
import { formatDateForUrl } from '@services/timeService';
import Link from 'next/link';

type WeekViewProps = {
  site: Site;
  week: dayjs.Dayjs[];
  blocks: AvailabilityBlock[];
  copyDay: (day: Dayjs) => void;
  pasteDay: (day: Dayjs) => void;
  copyWeek: (day: Dayjs) => void;
  pasteWeek: (day: Dayjs) => void;
};

const WeekView = ({
  site,
  week,
  blocks,
  copyDay,
  pasteDay,
  copyWeek,
  pasteWeek,
}: WeekViewProps) => {
  const [copyWeekText, setCopyWeekText] = React.useState('Copy week');

  const wrappedCopy = () => {
    copyWeek(week[0]);
    setCopyWeekText('Week copied');
    setTimeout(() => setCopyWeekText('Copy week'), 1000);
  };

  const days = [];
  for (let i = 0; i < 7; i++) days.push(week[0].add(i, 'day'));

  return (
    <>
      <div>
        <a href="#" onClick={wrappedCopy}>
          {copyWeekText}
        </a>
        <a
          href="#"
          onClick={() => pasteWeek(week[0])}
          style={{ marginLeft: '24px' }}
        >
          Paste week
        </a>
      </div>
      <ul className="nhsuk-grid-row nhsuk-card-group">
        {days.map(d => (
          <li
            key={d.format('DD ddd')}
            className="nhsuk-grid-column-full nhsuk-card-group__item"
          >
            <DayCard
              site={site}
              day={d}
              blocks={blocks}
              copyDay={copyDay}
              pasteDay={pasteDay}
            />
          </li>
        ))}
      </ul>
    </>
  );
};

type DayCardProps = {
  site: Site;
  day: dayjs.Dayjs;
  blocks: AvailabilityBlock[];
  copyDay: (day: Dayjs) => void;
  pasteDay: (day: Dayjs) => void;
};

const DayCard = ({ site, day, blocks, copyDay, pasteDay }: DayCardProps) => {
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

  // const defaultBlock = {
  //   day,
  //   start: '09:00',
  //   end: '12:00',
  //   appointmentLength: 5,
  //   sessionHolders: 1,
  //   services: [],
  //   isPreview: true,
  // };

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
          <Link
            href={`/site/${site.id}/availability/day?date=${formatDateForUrl(day)}`}
          >
            Edit
          </Link>
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
