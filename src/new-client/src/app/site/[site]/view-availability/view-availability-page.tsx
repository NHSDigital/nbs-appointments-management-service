import { Card, Table } from '@components/nhsuk-frontend';
import { AvailabilityResponse, Week } from '@types';
import dayjs from 'dayjs';

type Props = {
  availability: AvailabilityResponse[];
  weeks: Week[];
};

export const ViewAvailabilityPage = ({ availability, weeks }: Props) => {
  const mockRows = [
    ['Row 1 Col 1', 'Row 1 Col 2'],
    ['Row 2 Col 1', 'Row 2 Col 2'],
  ];

  return (
    <>
      {weeks.map((week, i) => (
        <Card
          title={`${week.start} ${dayjs().month(week.startMonth).format('MMMM')} to ${week.end} ${dayjs().month(week.endMonth).format('MMMM')}`}
          key={i}
        >
          <Table
            headers={['Services', 'Booked appointments']}
            rows={mockRows}
          ></Table>
        </Card>
      ))}
    </>
  );
};
