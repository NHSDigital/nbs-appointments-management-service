import dayjs from 'dayjs';

type Props = {
  date: dayjs.Dayjs;
};

const MonthViewCard = ({ date }: Props) => {
  const appointsmentsBooked = Math.floor(Math.random() * 301);
  const percentageBooked = Math.round((appointsmentsBooked / 300) * 100);
  const percentageOfPixels = Math.round((percentageBooked / 100) * 50);

  const isWeekend = date.day() === 0 || date.day() === 6;

  return (
    <div
      className={`nhsuk-card nhsuk-card--primary`}
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
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default MonthViewCard;
