import { formatDateForUrl } from '@services/timeService';
import { Site } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';

type Props = {
  date: dayjs.Dayjs;
  site: Site;
};

const MonthViewCard = ({ date, site }: Props) => {
  const appointsmentsBooked = Math.floor(Math.random() * 301);
  const percentageBooked = Math.round((appointsmentsBooked / 300) * 100);
  const percentageOfPixels = Math.round((percentageBooked / 100) * 50);

  const isWeekend = date.day() === 0 || date.day() === 6;

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
            {!isWeekend && (
              <span style={{ flexGrow: 1, textAlign: 'right' }}>
                {percentageBooked}%
              </span>
            )}
          </div>
          {isWeekend ? (
            <>
              <div
                style={{
                  flexGrow: 1,
                  background: 'grey',
                  opacity: 0,
                  height: '50px',
                  borderBottom: `${percentageOfPixels}px solid darkblue`,
                  marginBottom: 5,
                  borderRadius: 5,
                }}
              >
                &nbsp;
              </div>
              <div style={{ textWrap: 'nowrap' }}>Site is closed</div>
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'flex-start',
                  marginBottom: 5,
                }}
              >
                {!isWeekend && (
                  <Link
                    href={`/site/${site.id}/availability/day?date=${formatDateForUrl(date)}`}
                  >
                    Day
                  </Link>
                )}

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
                  background: 'grey',
                  height: '50px',
                  borderBottom: `${percentageOfPixels}px solid darkblue`,
                  marginBottom: 5,
                  borderRadius: 5,
                }}
              >
                &nbsp;
              </div>
              <div style={{ textWrap: 'nowrap' }}>
                {appointsmentsBooked} / 300
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
                {!isWeekend && (
                  <span style={{ flexGrow: 1, textAlign: 'right' }}>
                    <Link
                      href={`/site/${site.id}/availability/week?date=${formatDateForUrl(date)}`}
                    >
                      Week
                    </Link>
                  </span>
                )}
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default MonthViewCard;
