import { When } from '@components/when';
import { serviceSummary } from './services';
import { AvailabilityBlock } from '@types';

type SummaryAction = {
  title: string;
  action: (block: AvailabilityBlock) => void;
};

export type DaySummaryProps = {
  actionProvider: (block: AvailabilityBlock) => SummaryAction | undefined;
  blocks: AvailabilityBlock[];
};

export const DaySummary = ({ blocks, actionProvider }: DaySummaryProps) => {
  return (
    <dl className="nhsuk-summary-list">
      {blocks.map((b, i) => {
        const action = actionProvider(b);
        return (
          <div key={i} className="nhsuk-summary-list__row">
            <dt className="nhsuk-summary-list__key">
              {b.isPreview ? '*' : ''}
              {b.start} - {b.end}
            </dt>
            <dd className="nhsuk-summary-list__value">
              {serviceSummary(b.services)}
            </dd>
            <dd className="nhsuk-summary-list__actions">
              <When condition={action !== undefined}>
                <a href="#" onClick={() => action?.action(b)}>
                  {action?.title}
                  <span className="nhsuk-u-visually-hidden"> name</span>
                </a>
              </When>
            </dd>
          </div>
        );
      })}
    </dl>
  );
};
