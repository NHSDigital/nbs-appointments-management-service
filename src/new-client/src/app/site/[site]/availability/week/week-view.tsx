import { AvailabilityBlock, WeekInfo } from '@types';
import { timeSort } from './common';
import dayjs from 'dayjs';
import { DaySummary } from '../day-summary';

type WeekViewProps = {
  onAddBlock: (block: AvailabilityBlock) => void;
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
              <DayCard day={d} blocks={dayBlocks(d)} action={onAddBlock} />
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
};

const DayCard = ({ day, action, blocks }: DayCardProps) => {
  const defaultBlock = {
    day,
    start: '09:00',
    end: '12:00',
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
          blocks={blocks}
          actionProvider={() => ({ title: 'Change', action })}
        />
        <a href="#" onClick={() => action(defaultBlock)}>
          Add a time block
        </a>
      </div>
    </div>
  );
};

export default WeekView;
