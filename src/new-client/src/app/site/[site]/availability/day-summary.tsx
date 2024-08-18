import { When } from '@components/when';
import { serviceSummary } from './services';
import { AvailabilityBlock } from '@types';

type SummaryAction = {
  title: string;
  action: (block: AvailabilityBlock) => void;
  test: (block: AvailabilityBlock) => boolean;
};

export type DaySummaryProps = {
  primaryAction?: SummaryAction;
  secondaryAction?: SummaryAction;
  hasError: (block: AvailabilityBlock) => boolean;
  blocks: AvailabilityBlock[];
};

export const DaySummary = ({
  blocks,
  primaryAction,
  secondaryAction,
  hasError,
}: DaySummaryProps) => {
  return (
    <dl className="nhsuk-summary-list">
      {blocks.map((b, i) => {
        return (
          <div key={i} className="nhsuk-summary-list__row">
            <dt className={`nhsuk-summary-list__key `}>
              <div
                className={`${b.isPreview ? 'selected' : ''} ${
                  hasError(b)
                    ? 'nhsuk-form-group--error nhsuk-error-message'
                    : ''
                }`}
              >
                {b.start} - {b.end}
              </div>
            </dt>
            <dd className="nhsuk-summary-list__value">
              {serviceSummary(b.services)}
            </dd>
            <When
              condition={primaryAction !== undefined && primaryAction.test(b)}
            >
              <dd className="nhsuk-summary-list__actions">
                <a href="#" onClick={() => primaryAction?.action(b)}>
                  {primaryAction?.title}
                  <span className="nhsuk-u-visually-hidden"> name</span>
                </a>
              </dd>
            </When>
            <When
              condition={
                secondaryAction !== undefined && secondaryAction.test(b)
              }
            >
              <dd className="nhsuk-summary-list__actions">
                <a href="#" onClick={() => secondaryAction?.action(b)}>
                  {secondaryAction?.title}
                  <span className="nhsuk-u-visually-hidden"> name</span>
                </a>
              </dd>
            </When>
          </div>
        );
      })}
    </dl>
  );
};
