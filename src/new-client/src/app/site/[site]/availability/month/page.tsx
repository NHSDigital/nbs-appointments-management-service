const MonthOverviewPage = () => {
  const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  const dates = [
    [30, 7, 14, 21, 28],
    [1, 8, 15, 22, 29],
    [2, 9, 16, 23, 30],
    [3, 10, 17, 24, 31],
    [4, 11, 18, 25, 1],
    [5, 12, 19, 26, 2],
    [6, 13, 20, 27, 3],
  ];

  return (
    <div
      style={{
        flexDirection: 'column',
        margin: 'auto',
        display: 'flex',
        width: '960px',
        justifyContent: 'space-evenly',
      }}
    >
      <div
        style={{
          display: 'flex',
          alignItems: 'baseline',
          justifyContent: 'flex-start',
        }}
      >
        <h2 style={{ flexGrow: 1 }}>October 2024</h2>
        <div>70% - 6750 / 9000 capacity</div>
        <div style={{ marginLeft: '30px' }}>2250 free slots available</div>
      </div>
      <div
        style={{
          margin: 'auto',
          display: 'flex',
          width: '960px',
          justifyContent: 'space-evenly',
        }}
      >
        {days.map((d, i) => (
          <DayColumn key={d} day={d} dates={dates[i]} />
        ))}
      </div>
    </div>
  );
};

const DayColumn = ({ day, dates }: { day: string; dates: number[] }) => {
  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        margin: '2px',
      }}
    >
      <div>
        <h4>{day}</h4>
      </div>
      {dates.map(d => (
        <div
          key={d}
          className="nhsuk-card-group__item"
          style={{ width: '100%', marginTop: '5px' }}
        >
          <div className="nhsuk-card nhsuk-card" style={{ padding: '8px' }}>
            <div
              className="nhsuk-card__content"
              style={{
                padding: '2px',
                display: 'flex',
                justifyContent: 'flex-start',
                flexDirection: 'column',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'flex-start' }}>
                <span>
                  <b>{d}</b>
                </span>
                <span style={{ flexGrow: 1, textAlign: 'right' }}>78%</span>
              </div>
              <div
                style={{
                  flexGrow: 1,
                  background: 'grey',
                  height: '50px',
                  borderBottom: '39px solid darkblue',
                }}
              >
                &nbsp;
              </div>
              <div style={{ textWrap: 'nowrap' }}>200 / 300</div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default MonthOverviewPage;
