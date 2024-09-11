'use client';
import { When } from '@components/when';
import {
  calculateNumberOfAppointments,
  summariseServices,
} from '@services/availabilityService';
import { AvailabilityBlock } from '@types';
import React from 'react';

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
  showBreaks: boolean;
};

export const DaySummary = ({
  blocks,
  primaryAction,
  secondaryAction,
  hasError,
  showBreaks,
}: DaySummaryProps) => {
  const blocksToShow = React.useMemo(
    () => blocks.filter(b => showBreaks || !b.isBreak),
    [blocks, showBreaks],
  );

  return (
    <dl className="nhsuk-summary-list">
      {blocksToShow.map((b, i) => {
        return (
          <div
            key={i}
            className="nhsuk-summary-list__row"
            style={{
              backgroundColor: b.isPreview ? '#CAD7F5' : 'white',
              color: b.isBreak ? 'gray' : 'black',
              fontStyle: b.isBreak ? 'italic' : 'normal',
            }}
          >
            <dt
              className={`nhsuk-summary-list__key`}
              style={{ paddingLeft: b.isBreak ? '24px' : '0px' }}
            >
              <div
                className={`${
                  hasError(b)
                    ? 'nhsuk-form-group--error nhsuk-error-message'
                    : ''
                }`}
              >
                {b.start} - {b.end}
              </div>
            </dt>
            <dd className="nhsuk-summary-list__value">
              {b.isBreak
                ? 'Break Period'
                : summariseServices(b.services, 'No services selected')}
            </dd>
            <dd className="nhsuk-summary-list__value">
              <When condition={!b.isBreak}>
                {calculateNumberOfAppointments(b, blocks)} appointments
              </When>
            </dd>

            <dd className="nhsuk-summary-list__actions">
              <When
                condition={primaryAction !== undefined && primaryAction.test(b)}
              >
                <a
                  href="#"
                  onClick={() => primaryAction?.action(b)}
                  style={{ fontStyle: 'normal' }}
                >
                  {primaryAction?.title}
                  <span className="nhsuk-u-visually-hidden"> name</span>
                </a>
              </When>
            </dd>

            <dd className="nhsuk-summary-list__actions">
              <When
                condition={
                  secondaryAction !== undefined && secondaryAction.test(b)
                }
              >
                <a
                  href="#"
                  onClick={() => secondaryAction?.action(b)}
                  style={{ fontStyle: 'normal' }}
                >
                  {secondaryAction?.title}
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
