import { AvailabilityBlock, WeekInfo } from '@types';
import { timeSort } from './common';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';
import { DayCardProps, DaySummary } from '../day-summary';

type WeekViewProps = {
  onAddBlock: (day: dayjs.Dayjs, block?: string) => void;
  blocks: AvailabilityBlock[];
  week: WeekInfo;
};

const WeekView = ({ onAddBlock, blocks, week }: WeekViewProps) => {
  const days = [];

  for (let i = 0; i < 7; i++) days.push(week.commencing.add(i, 'day'));

  const dayBlocks = (d: dayjs.Dayjs) =>
    blocks.filter(b => b.day.isSame(d)).sort(timeSort);

  return (
    <>
      <h2>
        Availability for Week Commencing {week.commencing.format('MMMM DD')}
      </h2>
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
              <DayCard day={d} blocks={dayBlocks(d)} onAddBlock={onAddBlock} />
            </li>
          ))}
        </ul>
      </div>
    </>
  );
};

const DayCard = ({ day, onAddBlock, blocks }: DayCardProps) => {
  return (
    <div className="nhsuk-card nhsuk-card">
      <div className="nhsuk-card__content nhsuk-card__content--primary">
        <h2 className="nhsuk-card__heading nhsuk-heading-m">
          {day.format('DD ddd')}
        </h2>
        <DaySummary blocks={blocks} onAddBlock={onAddBlock} day={day} />
        <a href="#" onClick={() => onAddBlock(day)}>
          Add a time block
        </a>
      </div>
    </div>
  );
};

export default WeekView;
