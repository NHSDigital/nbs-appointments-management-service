type Props = {
  total: number;
  perHour?: number;
};

const AppointmentsSummaryText = ({ total, perHour }: Props) => {
  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'flex-start',
      }}
    >
      <span>
        Availability for <b>{total}</b> appointments
      </span>
      {perHour && (
        <>
          <span style={{ marginLeft: 32, marginRight: 32 }}>|</span>
          <span>
            Up to <b>{perHour}</b> appointments per hour
          </span>
        </>
      )}
    </div>
  );
};

export default AppointmentsSummaryText;
