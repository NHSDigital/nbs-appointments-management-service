import {
  DayJsType,
  getWeek,
  parseDateComponentsToUkDatetime,
  RFC3339Format,
  ukNow,
} from '@services/timeService';
import { AvailabilitySetup, BookingSetup, test } from '../../fixtures-v2';

test.describe('View Month Availability', () => {
  // ['Europe/London', 'UTC', 'Pacific/Kiritimati', 'Etc/GMT+12']
  ['Europe/London'].forEach(timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      test('All the month page data is arranged in the week cards as expected - March', async ({
        setup,
        page,
        monthViewAvailabilityPage,
      }) => {
        //target data for test across a 3 week period spanning end of March,
        //guarantees DST boundary crossing every year
        const nextYear = ukNow().year() + 1;
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

        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const weekOne = getWeek(twentyFourMarch!);
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const weekTwo = getWeek(thirtyOneMarch!);
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
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
          'February 2027',
          'April 2027',
        );

        const expectedWeekOverviews = [
          {
            header: '1 March to 7 March',
            sessions: [],
            totalAppointments: 0,
            booked: 0,
            unbooked: 0,
          },
          {
            header: '8 March to 14 March',
            sessions: [],
            totalAppointments: 0,
            booked: 0,
            unbooked: 0,
          },
          {
            header: '15 March to 21 March',
            sessions: [],
            totalAppointments: 0,
            booked: 0,
            unbooked: 0,
          },
          {
            header: '22 March to 28 March',
            sessions: [{ serviceName: 'RSV Adult', bookedAppointments: 4 }],
            totalAppointments: 480,
            booked: 4,
            unbooked: 476,
          },
          {
            header: '29 March to 4 April',
            sessions: [{ serviceName: 'RSV Adult', bookedAppointments: 2 }],
            totalAppointments: 240,
            booked: 2,
            unbooked: 238,
          },
        ];

        for (let i = 0; i < expectedWeekOverviews.length; i++) {
          await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
            expectedWeekOverviews[i],
          );
        }
      });

      //   test('All the month page data is arranged in the week cards as expected - Apr 2027', async () => {
      //     //go to a specific month page that has a daylight savings change
      //     await page.goto(
      //       `manage-your-appointments/site/${site.id}/view-availability?date=2027-04-01`,
      //     );
      //     await page.waitForURL(
      //       `**/site/${site.id}/view-availability?date=2027-04-01`,
      //     );
      //     await page.waitForSelector('.nhsuk-loader', {
      //       state: 'detached',
      //     });

      //     await monthViewAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
      //       'March 2027',
      //       'May 2027',
      //     );

      //     const expectedWeekOverviews = [
      //       {
      //         header: '29 March to 4 April',
      //         sessions: [{ serviceName: 'RSV Adult', bookedAppointments: 2 }],
      //         totalAppointments: 240,
      //         booked: 2,
      //         unbooked: 238,
      //       },
      //       {
      //         header: '5 April to 11 April',
      //         sessions: [],
      //         totalAppointments: 0,
      //         booked: 0,
      //         unbooked: 0,
      //       },
      //       {
      //         header: '12 April to 18 April',
      //         sessions: [],
      //         totalAppointments: 0,
      //         booked: 0,
      //         unbooked: 0,
      //       },
      //       {
      //         header: '19 April to 25 April',
      //         sessions: [{ serviceName: 'RSV Adult', bookedAppointments: 2 }],
      //         totalAppointments: 420,
      //         booked: 2,
      //         unbooked: 418,
      //       },
      //       {
      //         header: '26 April to 2 May',
      //         sessions: [{ serviceName: 'RSV Adult', bookedAppointments: 4 }],
      //         totalAppointments: 840,
      //         booked: 4,
      //         unbooked: 836,
      //       },
      //     ];

      //     for (let i = 0; i < expectedWeekOverviews.length; i++) {
      //       await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
      //         expectedWeekOverviews[i],
      //       );
      //     }
      //   });
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
          slotLength: 5,
          capacity: 5,
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
      durationMins: 5,
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
      fromTime: '16:55:00',
      durationMins: 5,
      service: 'RSV Adult',
      status: 'Booked',
      availabilityStatus: 'Supported',
    } as BookingSetup;
  });
};
