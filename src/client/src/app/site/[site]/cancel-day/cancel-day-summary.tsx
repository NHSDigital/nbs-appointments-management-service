import { InsetText } from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import { parseToUkDatetime } from '@services/timeService';
import { DaySummaryV2, ClinicalService } from '@types';

type Props = {
  date: string;
  daySummary: DaySummaryV2;
  clinicalServices: ClinicalService[];
};

const CancelDaySummary = ({ date, daySummary, clinicalServices }: Props) => {
  const parsedDate = parseToUkDatetime(date);

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={daySummary.sessionSummaries}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        tableCaption={`Sessions for ${parsedDate.format('dddd D MMMM')}`}
      />
      <InsetText>
        {daySummary.totalSupportedAppointments +
          daySummary.totalOrphanedAppointments}{' '}
        booked appointments will be cancelled. We'll notify people that their
        appointment has been cancelled
      </InsetText>
    </>
  );
};

export default CancelDaySummary;
