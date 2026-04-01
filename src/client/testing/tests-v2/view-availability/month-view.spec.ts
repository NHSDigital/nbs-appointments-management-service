/* eslint-disable @typescript-eslint/no-non-null-assertion */
import {
  DayJsType,
  getUkWeeksOfTheMonth,
  getWeek,
  parseDateComponentsToUkDatetime,
  RFC3339Format,
  ukNow,
} from '@services/timeService';
import { AvailabilitySetup, BookingSetup, test } from '../../fixtures-v2';
import { WeekOverview } from '../../availability';

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
            //target data for test across a 3 week period spanning the end of March,
            //this guarantees DST boundary is crossed with some data before, on, and after the boundary
            const nextYear = ukNow().year() + yearIncrement;

            const twentyFourMarch = parseDateComponentsToUkDatetime({
              day: 24,
              month: 3,
              year: nextYear,
            });
            const thirtyOneMarch = parseDateComponentsToUkDatetime({
              day: 31,
              month: 3,
              year: nextYear,
            });
            const sevenApril = parseDateComponentsToUkDatetime({
              day: 7,
              month: 4,
              year: nextYear,
            });

            const weekOne = getWeek(twentyFourMarch!);
            const weekTwo = getWeek(thirtyOneMarch!);
            const weekThree = getWeek(sevenApril!);

            const allWeeks = weekOne.concat(weekTwo).concat(weekThree);

            const availability = mapToSessions(allWeeks);
            const firstBookings = mapToFirstBookings(allWeeks);
            const lastBookings = mapToLastBookings(allWeeks);

            //go to a specific month page that has a daylight savings change
            const { site } = await setup({
              availability: availability,
              bookings: firstBookings.concat(lastBookings),
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

            const marchWeeks = getUkWeeksOfTheMonth(thirtyOneMarch!);

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

            const aprilWeeks = getUkWeeksOfTheMonth(sevenApril!);

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

            const twentyFourOctober = parseDateComponentsToUkDatetime({
              day: 24,
              month: 10,
              year: nextYear,
            });
            const thirtyOneOctober = parseDateComponentsToUkDatetime({
              day: 31,
              month: 10,
              year: nextYear,
            });
            const sevenNovember = parseDateComponentsToUkDatetime({
              day: 7,
              month: 11,
              year: nextYear,
            });

            const weekOne = getWeek(twentyFourOctober!);
            const weekTwo = getWeek(thirtyOneOctober!);
            const weekThree = getWeek(sevenNovember!);

            const allWeeks = weekOne.concat(weekTwo).concat(weekThree);

            const availability = mapToSessions(allWeeks);
            const firstBookings = mapToFirstBookings(allWeeks);
            const lastBookings = mapToLastBookings(allWeeks);

            //go to a specific month page that has a daylight savings change
            const { site } = await setup({
              availability: availability,
              bookings: firstBookings.concat(lastBookings),
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

            const octoberWeeks = getUkWeeksOfTheMonth(thirtyOneOctober!);

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

            const novemberWeeks = getUkWeeksOfTheMonth(sevenNovember!);

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

const mapToSessions = (days: DayJsType[]): AvailabilitySetup[] => {
  return days.map(day => {
    return {
      date: day.format(RFC3339Format),
      sessions: [
        {
          from: '09:00',
          until: '17:00',
          services: ['RSV Adult'],
          slotLength: 10,
          capacity: 2,
        },
      ],
    } as AvailabilitySetup;
  });
};

const mapToFirstBookings = (days: DayJsType[]): BookingSetup[] => {
  return days.map(day => {
    return {
      fromDate: day.format(RFC3339Format),
      fromTime: '09:00:00',
      durationMins: 10,
      service: 'RSV Adult',
      status: 'Booked',
      availabilityStatus: 'Supported',
    } as BookingSetup;
  });
};

const mapToLastBookings = (days: DayJsType[]): BookingSetup[] => {
  return days.map(day => {
    return {
      fromDate: day.format(RFC3339Format),
      fromTime: '16:50:00',
      durationMins: 10,
      service: 'RSV Adult',
      status: 'Booked',
      availabilityStatus: 'Supported',
    } as BookingSetup;
  });
};

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
