import NhsPage from '@components/nhs-page';
import { fetchAvailability, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { ViewAvailabilityPage } from './view-availability-page';
import { AvailabilityBlock, FetchAvailabilityRequest, Week } from '@types';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const title = `View availability for ${dayjs().format('MMMM YYYY')}`;

  // TODO: Look into optimising these methods as there is a chunk of duplicated code
  const getWeeksInMonth = (year: number, month: number): Week[] => {
    const weeks: Week[] = [];

    const firstDate = new Date(year, month, 1);
    const lastDate = new Date(year, month + 1, 0);
    const numDays = lastDate.getDate();
    const dayOfWeekCounter = firstDate.getDay();

    let start = 1;
    let end = 7;
    if (dayOfWeekCounter === 0) {
      end = 1;
    } else {
      end = 7 - dayOfWeekCounter + 1;
    }

    while (start <= numDays) {
      weeks.push({
        start: start,
        end: end,
        startMonth: month,
        endMonth: month,
        year: year,
      });
      start = end + 1;
      end = end + 7;
      end = start === 1 && end === 8 ? 1 : end;
      if (end > numDays) {
        end = numDays;
      }
    }

    if (weeks[0].start === 1) {
      const beforeIndex = addMonth(year, month - 1, 1);
      weeks[0].start = beforeIndex.start;
      weeks[0].startMonth = beforeIndex.startMonth;
    }

    if (weeks[weeks.length - 1].end === numDays) {
      const afterIndex = addMonth(year, month + 1, 0);
      weeks[weeks.length - 1].end = afterIndex.start;
      weeks[weeks.length - 1].endMonth = afterIndex.endMonth;
    }

    return weeks;
  };

  const addMonth = (year: number, month: number, flag: number): Week => {
    const weeks: Week[] = [];

    const firstDate = new Date(year, month, 1);
    const lastDate = new Date(year, month + 1, 0);
    const numDays = lastDate.getDate();
    const dayOfWeekCounter = firstDate.getDay();

    let start = 1;
    let end = 7;
    if (dayOfWeekCounter === 0) {
      end = 1;
    } else {
      end = 7 - dayOfWeekCounter + 1;
    }

    while (start <= numDays) {
      weeks.push({
        start: start,
        end: end,
        startMonth: month,
        endMonth: month,
        year: year,
      });
      start = end + 1;
      end = end + 7;
      end = start === 1 && end === 8 ? 1 : end;
      if (end > numDays) {
        end = numDays;
      }
    }

    if (flag === 0) {
      return weeks[0];
    }

    if (flag === 1) {
      return weeks[weeks.length - 1];
    }

    return weeks[0];
  };
  const weeks = getWeeksInMonth(dayjs().year(), dayjs().month());

  const firstWeek = weeks[0];
  const lastWeek = weeks[weeks.length - 1];
  const startDate = new Date(
    firstWeek.year,
    firstWeek.startMonth,
    firstWeek.start,
  );
  const endDate = new Date(lastWeek.year, lastWeek.endMonth, lastWeek.end);
  const payload: FetchAvailabilityRequest = {
    sites: [site.id],
    service: '*',
    from: dayjs(startDate).format('YYYY-MM-DD'),
    until: dayjs(endDate).format('YYYY-MM-DD'),
    queryType: 'Days',
  };
  const availability = await fetchAvailability(payload);

  // TODO: Can this be optimised / cleaned up?
  const getUnbookedCount = () => {
    if (!availability || availability.length === 0) {
      return;
    }

    for (let w = 0; w < weeks.length; w++) {
      for (let a = 0; a < availability.length; a++) {
        if (availability[a].availability.length === 0) {
          continue;
        }

        const fromDate = new Date(
          weeks[w].year,
          weeks[w].startMonth,
          weeks[w].start,
        );
        const toDate = new Date(weeks[w].year, weeks[w].endMonth, weeks[w].end);

        const blocks: AvailabilityBlock[] = [];
        availability[a].availability.filter(item => {
          const date = new Date(item.date);
          if (
            date.getTime() >= fromDate.getTime() &&
            date.getTime() <= toDate.getTime()
          ) {
            blocks.push(...item.blocks);
          }
        });

        const countArray = blocks.map(b => b.count);
        weeks[w].unbooked = countArray.reduce((sum, num) => sum + num);
      }
    }
  };
  getUnbookedCount();

  return (
    <NhsPage
      title={title}
      caption={site.name}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
    >
      <ViewAvailabilityPage weeks={weeks} />
    </NhsPage>
  );
};

export default Page;
