import dayjs from 'dayjs';
import { serviceSummary } from './services';
import { AvailabilityBlock } from '@types';

export type DayCardProps = {
  onAddBlock: (day: dayjs.Dayjs, block?: string) => void;
  day: dayjs.Dayjs;
  blocks: AvailabilityBlock[];
};

export const DaySummary = ({ blocks, onAddBlock, day }: DayCardProps) => {
  return (
    <dl className="nhsuk-summary-list">
      {blocks.map((b, i) => (
        <div key={i} className="nhsuk-summary-list__row">
          <dt className="nhsuk-summary-list__key">
            {b.start} - {b.end}
          </dt>
          <dd className="nhsuk-summary-list__value">
            {serviceSummary(b.services)}
          </dd>
          <dd className="nhsuk-summary-list__actions">
            <a href="#" onClick={() => onAddBlock(day, b.start)}>
              Change<span className="nhsuk-u-visually-hidden"> name</span>
            </a>
          </dd>
        </div>
      ))}
    </dl>
  );
};
