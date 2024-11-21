import { Card, Table } from '@components/nhsuk-frontend';
import { AvailabilityResponse, Week } from '@types';

type Props = {
  availability: AvailabilityResponse[];
  weeks: Week[];
};

export const ViewAvailabilityPage = ({ availability, weeks }: Props) => {
  const mockRows = [
    ['Row 1 Col 1', 'Row 1 Col 2'],
    ['Row 2 Col 1', 'Row 2 Col 2'],
  ];

  console.log(weeks);

  return (
    <>
      {weeks.map((week, i) => (
        <Card
          title={`${week.start} ${week.startMonth} to ${week.end} ${week.endMonth}`}
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
