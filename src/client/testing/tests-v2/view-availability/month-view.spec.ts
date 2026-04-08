import { DayJsType, getUkWeeksOfTheMonth, ukNow } from '@services/timeService';
import { test } from '../../fixtures-v2';
import {
  clockBackwardWeeksData,
  clockForwardWeeksData,
  WeekOverview,
} from '../../availability';

test.describe('View Month Availability', () => {
  ['Europe/London', 'Asia/Kamchatka', 'US/Pacific'].forEach(async timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      [0, 1, 2].forEach(yearIncrement => {
        test.describe(`Test in year: '${ukNow().year() + yearIncrement}'`, () => {
          test('All the month page data is arranged in the week cards as expected - Clocks Forward DST', async ({
            setup,
            page,
            monthViewAvailabilityPage,
          }) => {
            const nextYear = ukNow().year() + yearIncrement;
            const data = clockForwardWeeksData(nextYear);

            //go to a specific month page that has a daylight savings change
            const { site } = await setup({
              ...data,
            });

            //next year march
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability?date=${nextYear}-03-01`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability?date=${nextYear}-03-01`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            await monthViewAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
              `February ${nextYear}`,
              `April ${nextYear}`,
            );

            const marchWeeks = getUkWeeksOfTheMonth(data.firstDate);

            //expect last 2 week cards to have data, none of the rest should!
            const lastTwoWeeks = marchWeeks.slice(marchWeeks.length - 2);
            const restOfWeeks = marchWeeks.slice(undefined, -2);

            const noDataCards1 = noSessionsInWeeks(restOfWeeks);
            const dataCards1 = dataInWeeks(lastTwoWeeks);

            const expectedWeekOverviews1 = noDataCards1.concat(dataCards1);

            for (let i = 0; i < expectedWeekOverviews1.length; i++) {
              await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
                expectedWeekOverviews1[i],
              );
            }

            //next year april
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability?date=${nextYear}-04-01`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability?date=${nextYear}-04-01`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            await monthViewAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
              `March ${nextYear}`,
              `May ${nextYear}`,
            );

            const aprilWeeks = getUkWeeksOfTheMonth(data.lastDate);

            let cardsWithData = 2;

            //if first week is a April date (i.e no overlap with March), then only 1 card to check
            //months are 0 indexed...
            if (aprilWeeks[0][0].month() === 3) {
              cardsWithData = 1;
            }

            //expect first 2 week cards to have data, none of the rest should!
            const firstTwoWeeks = aprilWeeks.slice(0, cardsWithData);
            const otherWeeks = aprilWeeks.slice(cardsWithData);

            const noDataCards2 = noSessionsInWeeks(otherWeeks);
            const dataCards2 = dataInWeeks(firstTwoWeeks);

            const expectedWeekOverviews2 = dataCards2.concat(noDataCards2);

            for (let i = 0; i < expectedWeekOverviews2.length; i++) {
              await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
                expectedWeekOverviews2[i],
              );
            }
          });

          test('All the month page data is arranged in the week cards as expected - Clocks Backward DST', async ({
            setup,
            page,
            monthViewAvailabilityPage,
          }) => {
            //target data for test across a 3 week period spanning the end of October,
            //this guarantees DST boundary is crossed with some data before, on, and after the boundary
            const nextYear = ukNow().year() + yearIncrement;
            const data = clockBackwardWeeksData(nextYear);

            //go to a specific month page that has a daylight savings change
            const { site } = await setup({
              ...data,
            });

            //next year march
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability?date=${nextYear}-10-01`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability?date=${nextYear}-10-01`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            await monthViewAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
              `September ${nextYear}`,
              `November ${nextYear}`,
            );

            const octoberWeeks = getUkWeeksOfTheMonth(data.firstDate);

            //expect last 2 week cards to have data, none of the rest should!
            const lastTwoWeeks = octoberWeeks.slice(octoberWeeks.length - 2);
            const restOfWeeks = octoberWeeks.slice(undefined, -2);

            const noDataCards1 = noSessionsInWeeks(restOfWeeks);
            const dataCards1 = dataInWeeks(lastTwoWeeks);

            const expectedWeekOverviews1 = noDataCards1.concat(dataCards1);

            for (let i = 0; i < expectedWeekOverviews1.length; i++) {
              await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
                expectedWeekOverviews1[i],
              );
            }

            //next year november
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability?date=${nextYear}-11-01`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability?date=${nextYear}-11-01`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            await monthViewAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
              `October ${nextYear}`,
              `December ${nextYear}`,
            );

            const novemberWeeks = getUkWeeksOfTheMonth(data.lastDate);

            let cardsWithData = 2;

            //if first week is a November date (i.e no overlap with Oct), then only 1 card to check
            //months are 0 indexed...
            if (novemberWeeks[0][0].month() === 10) {
              cardsWithData = 1;
            }

            //expect first 2 week cards to have data, none of the rest should!
            const firstTwoWeeks = novemberWeeks.slice(0, cardsWithData);
            const otherWeeks = novemberWeeks.slice(cardsWithData);

            const noDataCards2 = noSessionsInWeeks(otherWeeks);
            const dataCards2 = dataInWeeks(firstTwoWeeks);

            const expectedWeekOverviews2 = dataCards2.concat(noDataCards2);

            for (let i = 0; i < expectedWeekOverviews2.length; i++) {
              await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
                expectedWeekOverviews2[i],
              );
            }
          });
        });
      });
    });
  });
});

const noSessionsInWeeks = (weeks: DayJsType[][]): WeekOverview[] => {
  return weeks.map(week => {
    const ukWeekStart = week[0];
    const ukWeekEnd = week[6];

    return {
      header: `${ukWeekStart.format('D MMMM')} to ${ukWeekEnd.format('D MMMM')}`,
      sessions: [],
      totalAppointments: 0,
      booked: 0,
      unbooked: 0,
    } as WeekOverview;
  });
};

const dataInWeeks = (weeks: DayJsType[][]): WeekOverview[] => {
  return weeks.map(week => {
    const ukWeekStart = week[0];
    const ukWeekEnd = week[6];

    return {
      header: `${ukWeekStart.format('D MMMM')} to ${ukWeekEnd.format('D MMMM')}`,
      sessions: [{ serviceName: 'RSV Adult', bookedAppointments: 14 }],
      totalAppointments: 672,
      booked: 14,
      unbooked: 658,
    } as WeekOverview;
  });
};
