'use client';
import { When } from '@components/when';
import { useAvailability } from '@hooks/useAvailability';
import { calculateNumberOfAppointments } from '@services/availabilityService';
import { formatDateForUrl, parseDate } from '@services/timeService';
import { Site } from '@types';
import Link from 'next/link';
import { useMemo } from 'react';

type Props = {
  dateString: string;
  site: Site;
};

const MonthViewCard = ({ dateString, site }: Props) => {
  const date = parseDate(dateString);
  const { blocks } = useAvailability();

  const slotCount = useMemo(() => {
    let slots = 0;
    blocks
      .filter(b => b.day.isSame(date))
      .forEach(b => {
        const slotsInBlock = calculateNumberOfAppointments(b, []);
        if (b.isBreak) slots -= slotsInBlock;
        else slots += slotsInBlock;
      });
    return slots;
  }, [blocks, date]);

  const appointsmentsBooked = useMemo(() => {
    if (Math.random() < 0.15) return slotCount;
    else return Math.floor(Math.random() * slotCount);
  }, [slotCount]);

  const percentageBooked = useMemo(
    () =>
      slotCount > 0 ? Math.round((appointsmentsBooked / slotCount) * 100) : 0,
    [slotCount, appointsmentsBooked],
  );

  const percentageOfPixels = useMemo(
    () => Math.round((percentageBooked / 100) * 50),
    [percentageBooked],
  );

  const isFull = useMemo(
    () => appointsmentsBooked === slotCount,
    [appointsmentsBooked, slotCount],
  );

  return (
    <div
      className={`nhsuk-card nhs-card--clickable nhsuk-card--primary`}
      style={{ padding: 10, marginBottom: 0 }}
    >
      <div
        className={`nhsuk-card__content nhsuk-card__content--primary`}
        style={{ padding: 0 }}
      >
        <div
          style={{
            display: 'flex',
            justifyContent: 'flex-start',
            flexDirection: 'column',
          }}
        >
          <div
            style={{
              display: 'flex',
              justifyContent: 'flex-start',
              marginBottom: 5,
            }}
          >
            <span>
              <b>{date.format('DD')}</b>
            </span>
            {slotCount > 0 && (
              <span style={{ flexGrow: 1, textAlign: 'right' }}>
                {percentageBooked}%
              </span>
            )}
          </div>
          {slotCount === 0 ? (
            <>
              <div
                style={{
                  flexGrow: 1,
                  height: '50px',
                  marginBottom: 5,
                  borderRadius: 5,
                }}
              >
                <svg height="50" width="100" xmlns="http://www.w3.org/2000/svg">
                  <g fill="none" stroke="lightgray">
                    <path strokeWidth="2" d="M42 35 l24 0" />
                  </g>
                </svg>
              </div>
              <div>&nbsp;</div>
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'flex-start',
                  marginBottom: 5,
                }}
              >
                <Link
                  href={`/site/${site.id}/availability/day?date=${formatDateForUrl(date)}`}
                >
                  Day
                </Link>

                <span style={{ flexGrow: 1, textAlign: 'right' }}>
                  <Link
                    href={`/site/${site.id}/availability/week?date=${formatDateForUrl(date)}`}
                  >
                    Week
                  </Link>
                </span>
              </div>
            </>
          ) : (
            <>
              <div
                style={{
                  flexGrow: 1,
                  background: '#d8dde0',
                  height: '50px',
                  borderBottom: `${percentageOfPixels}px solid #005eb8`,
                  marginBottom: 5,
                  borderRadius: 3,
                  textAlign: 'center',
                  color: 'lightgray',
                  verticalAlign: 'center',
                }}
              >
                <When condition={isFull}>
                  <div style={{ marginTop: '14px', fontWeight: 'bold' }}>
                    Full
                  </div>
                </When>
                <When condition={!isFull}>&nbsp;</When>
              </div>
              <div style={{ textWrap: 'nowrap' }}>
                {appointsmentsBooked} / {slotCount}
              </div>
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'flex-start',
                  marginBottom: 5,
                }}
              >
                <Link
                  href={`/site/${site.id}/availability/day?date=${formatDateForUrl(date)}`}
                >
                  Day
                </Link>

                <span style={{ flexGrow: 1, textAlign: 'right' }}>
                  <Link
                    href={`/site/${site.id}/availability/week?date=${formatDateForUrl(date)}`}
                  >
                    Week
                  </Link>
                </span>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default MonthViewCard;
