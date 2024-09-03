'use client';
import { When } from '@components/when';
import { serviceSummary } from './services';
import { AvailabilityBlock } from '@types';
import dayjs from 'dayjs';
import React from 'react';
import { isWithin } from './week/common';

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
  const calculateNumberOfAppointments = (block: AvailabilityBlock): number => {
    const startDateTime = block.day.format('YYYY-MM-DD ') + block.start;
    const start = dayjs(startDateTime);

    const endDateTime = block.day.format('YYYY-MM-DD ') + block.end;
    const end = dayjs(endDateTime);
    const minutes = end.diff(start, 'minute');
    const unadjusted =
      (minutes / block.appointmentLength) * block.sessionHolders;

    if (!block.isBreak) {
      const breaks = blocks
        .filter(b => b.isBreak && isWithin(b, block))
        .map(b => {
          return calculateNumberOfAppointments({
            ...b,
            sessionHolders: block.sessionHolders,
            appointmentLength: block.appointmentLength,
          });
        });

      const reduction = breaks?.reduce((a, b) => a + b, 0) ?? 0;
      return unadjusted - reduction;
    }

    return unadjusted;
  };

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
              color: b.isBreak ? 'gray' : 'black',
              fontStyle: b.isBreak ? 'italic' : 'normal',
            }}
          >
            <dt
              className={`nhsuk-summary-list__key`}
              style={{ paddingLeft: b.isBreak ? '24px' : '0px' }}
            >
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
            <dd className="nhsuk-summary-list__value">
              <When condition={!b.isBreak}>
                {calculateNumberOfAppointments(b)} appointments
              </When>
            </dd>
            <When
              condition={primaryAction !== undefined && primaryAction.test(b)}
            >
              <dd className="nhsuk-summary-list__actions">
                <a
                  href="#"
                  onClick={() => primaryAction?.action(b)}
                  style={{ fontStyle: 'normal' }}
                >
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
                <a
                  href="#"
                  onClick={() => secondaryAction?.action(b)}
                  style={{ fontStyle: 'normal' }}
                >
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
