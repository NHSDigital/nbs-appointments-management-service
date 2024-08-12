import { AvailabilityBlock, WeekInfo } from '@types';
import { timeSort } from './common';
// eslint-disable-next-line import/no-extraneous-dependencies
import dayjs from 'dayjs';

type WeekViewProps = {
  onAddBlock: (day: dayjs.Dayjs) => void;
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
              <DayCard
                day={d}
                blocks={dayBlocks(d)}
                onAddBlock={() => onAddBlock(d)}
              />
            </li>
          ))}
        </ul>
      </div>
    </>
  );
};

type DayCardProps = {
  onAddBlock: () => void;
  day: dayjs.Dayjs;
  blocks: AvailabilityBlock[];
};

const DayCard = ({ day, onAddBlock, blocks }: DayCardProps) => {
  return (
    <div className="nhsuk-card nhsuk-card">
      <div className="nhsuk-card__content nhsuk-card__content--primary">
        <h2 className="nhsuk-card__heading nhsuk-heading-m">
          {day.format('DD ddd')}
        </h2>
        <DaySummary blocks={blocks} />
        <a href="#" onClick={onAddBlock}>
          Add a time block
        </a>
      </div>
    </div>
  );
};

const DaySummary = ({ blocks }: { blocks: AvailabilityBlock[] }) => {
  return (
    <dl className="nhsuk-summary-list">
      {blocks.map((b, i) => (
        <div key={i} className="nhsuk-summary-list__row">
          <dt className="nhsuk-summary-list__key">
            {b.start} - {b.end}
          </dt>
          <dd className="nhsuk-summary-list__value">
            {b.sessionHolders} session holders , {b.services} services
          </dd>
          <dd className="nhsuk-summary-list__actions">
            <a href="#">
              Change<span className="nhsuk-u-visually-hidden"> name</span>
            </a>
          </dd>
        </div>
      ))}
    </dl>
  );
};

export default WeekView;
