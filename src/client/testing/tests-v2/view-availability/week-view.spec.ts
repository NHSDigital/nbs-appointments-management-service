/* eslint-disable @typescript-eslint/no-non-null-assertion */
import {
  isBeforeCalendarDateUk,
  parseDateComponentsToUkDatetime,
  RFC3339Format,
  ukNow,
} from '@services/timeService';
import { AvailabilitySetup, BookingSetup, test } from '../../fixtures-v2';
import {
  clockBackwardWeeksData,
  clockForwardWeeksData,
} from '../../availability';

test.describe('View Week Availability', () => {
  ['Europe/London', 'Asia/Kamchatka', 'US/Pacific'].forEach(async timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      [2, 5, 8, 11].forEach(month => {
        //targeted date has to be within the next year (as needs session manipulations!)
        const now = ukNow();
        let firstOfMonth = parseDateComponentsToUkDatetime({
          day: 1,
          month: month,
          year: now.year(),
        });

        const isBefore = isBeforeCalendarDateUk(now, firstOfMonth!);

        //need to set target date to next year to keep within the allowed bounds
        if (!isBefore) {
          firstOfMonth = parseDateComponentsToUkDatetime({
            day: 1,
            month: month,
            year: now.year() + 1,
          });
        }

        test.describe(`Greedy shuffle: ${firstOfMonth!.format('dddd D MMMM')}`, () => {
          test('View week page data is arranged according to greedy model allocation, and the data shuffles on session changes', async ({
            setup,
            page,
            weekViewAvailabilityPage,
            addSessionPage,
            addServicesPage,
            checkSessionDetailsPage,
            changeAvailabilityPage,
          }) => {
            const rfcFormat = firstOfMonth!.format(RFC3339Format);

            const defaultSessions = [
              {
                date: firstOfMonth!.format(RFC3339Format),
                sessions: [
                  {
                    from: '11:00',
                    until: '12:00',
                    services: [
                      'COVID:5_11',
                      'RSV:Adult',
                      'FLU:18_64',
                      'COVID:18+',
                    ],
                    slotLength: 5,
                    capacity: 2,
                  },
                  {
                    from: '11:15',
                    until: '11:50',
                    services: ['COVID:5_11', 'FLU:18_64', 'RSV:Adult'],
                    slotLength: 5,
                    capacity: 2,
                  },
                ],
              },
            ] as AvailabilitySetup[];

            const defaultBookings = [
              {
                fromDate: rfcFormat,
                fromTime: '11:45:00',
                durationMins: 5,
                service: 'COVID:5_11',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
              {
                fromDate: rfcFormat,
                fromTime: '11:25:00',
                durationMins: 5,
                service: 'FLU:18_64',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
              {
                fromDate: rfcFormat,
                fromTime: '11:25:00',
                durationMins: 5,
                service: 'COVID_FLU:18_64',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
              {
                fromDate: rfcFormat,
                fromTime: '11:00:00',
                durationMins: 5,
                service: 'COVID_FLU:65+',
                status: 'Booked',
                availabilityStatus: 'Supported',
              },
            ] as BookingSetup[];

            const { site } = await setup({
              availability: defaultSessions,
              bookings: defaultBookings,
            });

            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${rfcFormat}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${rfcFormat}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            //verify default setup
            await weekViewAvailabilityPage.verifySessionDataDisplayedInTheCorrectOrder(
              {
                header: firstOfMonth!.format('dddd D MMMM'),
                booked: 4,
                unbooked: 36,
                orphaned: 2,
                totalAppointments: 40,
                sessions: [
                  {
                    serviceName: 'COVID 5-11RSV AdultFlu 18-64COVID 18+',
                    booked: '0 booked0 booked0 booked0 booked',
                    unbooked: 24,
                    sessionTimeInterval: '11:00 - 12:00',
                  },
                  {
                    serviceName: 'COVID 5-11Flu 18-64RSV Adult',
                    booked: '1 booked1 booked0 booked',
                    unbooked: 12,
                    sessionTimeInterval: '11:15 - 11:50',
                  },
                ],
              },
            );

            //create some new availability to show shuffling
            await page.goto(
              `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${rfcFormat}`,
            );
            await page.waitForURL(
              `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${rfcFormat}`,
            );

            await addSessionPage.addSession('10', '00', '12', '00', '2', '5');
            await addServicesPage.addService('Flu 18 to 64');
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${rfcFormat}`,
            );

            await weekViewAvailabilityPage.verifySessionDataDisplayedInTheCorrectOrder(
              {
                header: firstOfMonth!.format('dddd D MMMM'),
                booked: 4,
                unbooked: 84,
                orphaned: 2,
                totalAppointments: 88,
                sessions: [
                  {
                    serviceName: 'COVID 5-11RSV AdultFlu 18-64COVID 18+',
                    booked: '0 booked0 booked0 booked0 booked',
                    unbooked: 24,
                    sessionTimeInterval: '11:00 - 12:00',
                  },
                  {
                    serviceName: 'COVID 5-11Flu 18-64RSV Adult',
                    booked: '1 booked0 booked0 booked',
                    unbooked: 13,
                    sessionTimeInterval: '11:15 - 11:50',
                  },
                  {
                    serviceName: 'Flu 18-64',
                    booked: '1 booked',
                    unbooked: 47,
                    sessionTimeInterval: '10:00 - 12:00',
                  },
                ],
              },
            );

            //create some new availability to show shuffling
            await page.goto(
              `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${rfcFormat}`,
            );
            await page.waitForURL(
              `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${rfcFormat}`,
            );

            await addSessionPage.addSession('11', '05', '11', '55', '2', '5');
            await addServicesPage.addServices([
              'COVID 5 to 11',
              'Flu 18 to 64',
            ]);
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${rfcFormat}`,
            );

            await weekViewAvailabilityPage.verifySessionDataDisplayedInTheCorrectOrder(
              {
                header: firstOfMonth!.format('dddd D MMMM'),
                booked: 4,
                unbooked: 104,
                orphaned: 2,
                totalAppointments: 108,
                sessions: [
                  {
                    serviceName: 'COVID 5-11RSV AdultFlu 18-64COVID 18+',
                    booked: '0 booked0 booked0 booked0 booked',
                    unbooked: 24,
                    sessionTimeInterval: '11:00 - 12:00',
                  },
                  {
                    serviceName: 'COVID 5-11Flu 18-64RSV Adult',
                    booked: '0 booked0 booked0 booked',
                    unbooked: 14,
                    sessionTimeInterval: '11:15 - 11:50',
                  },
                  {
                    serviceName: 'Flu 18-64',
                    booked: '1 booked',
                    unbooked: 47,
                    sessionTimeInterval: '10:00 - 12:00',
                  },
                  {
                    serviceName: 'Flu 18-64COVID 5-11',
                    booked: '0 booked1 booked',
                    unbooked: 19,
                    sessionTimeInterval: '11:05 - 11:55',
                  },
                ],
              },
            );

            //remove the single Flu session
            const dayCard = page
              .getByRole('heading', {
                name: firstOfMonth!.format('dddd D MMMM'),
              })
              .locator('../..');

            //select the third session (flu only)
            const fluSessionRow = (
              await dayCard.getByRole('table').getByRole('row').all()
            )[3];

            const changeButton = fluSessionRow.getByRole('link', {
              name: 'Change',
            });

            await changeButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week/edit-session?date=${rfcFormat}&session**`,
            );

            await changeAvailabilityPage.selectChangeType('CancelSession');
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL(
              `**/site/${site.id}/availability/cancel/confirmation?session**`,
            );

            await changeAvailabilityPage.cancelSessionButton.click();

            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${rfcFormat}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${rfcFormat}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            await weekViewAvailabilityPage.verifySessionDataDisplayedInTheCorrectOrder(
              {
                header: firstOfMonth!.format('dddd D MMMM'),
                booked: 4,
                unbooked: 56,
                orphaned: 2,
                totalAppointments: 60,
                sessions: [
                  {
                    serviceName: 'COVID 5-11RSV AdultFlu 18-64COVID 18+',
                    booked: '0 booked0 booked0 booked0 booked',
                    unbooked: 24,
                    sessionTimeInterval: '11:00 - 12:00',
                  },
                  {
                    serviceName: 'COVID 5-11Flu 18-64RSV Adult',
                    booked: '0 booked0 booked0 booked',
                    unbooked: 14,
                    sessionTimeInterval: '11:15 - 11:50',
                  },
                  {
                    serviceName: 'Flu 18-64COVID 5-11',
                    booked: '1 booked1 booked',
                    unbooked: 18,
                    sessionTimeInterval: '11:05 - 11:55',
                  },
                ],
              },
            );
          });
        });
      });

      const forwardData = clockForwardWeeksData(ukNow().year());

      forwardData.weekTestCases.forEach(weekCase => {
        test.describe(`Week: '${weekCase.weekHeader}'`, () => {
          test.beforeEach(async ({ setup, page }) => {
            const { site } = await setup({ ...forwardData });

            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${weekCase.week}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${weekCase.week}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });
          });

          test(`View week page data is arranged in the day cards as expected`, async ({
            weekViewAvailabilityPage,
          }) => {
            // TODO add back in

            // await weekViewAvailabilityPage.verifyViewNextAndPreviousWeeksButtonsDisplayed(
            //   week.previousWeek,
            //   week.nextWeek,
            // );

            for (let i = 0; i < weekCase.dayOverviews.length; i++) {
              await weekViewAvailabilityPage.verifyAllDayCardInformationDisplayedCorrectly(
                weekCase.dayOverviews[i],
              );
            }
          });
        });
      });

      const backwardData = clockBackwardWeeksData(ukNow().year());

      backwardData.weekTestCases.forEach(weekCase => {
        test.describe(`Week: '${weekCase.weekHeader}'`, () => {
          test.beforeEach(async ({ setup, page }) => {
            const { site } = await setup({ ...backwardData });

            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${weekCase.week}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${weekCase.week}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });
          });

          test(`View week page data is arranged in the day cards as expected`, async ({
            weekViewAvailabilityPage,
          }) => {
            // TODO add back in

            // await weekViewAvailabilityPage.verifyViewNextAndPreviousWeeksButtonsDisplayed(
            //   week.previousWeek,
            //   week.nextWeek,
            // );

            for (let i = 0; i < weekCase.dayOverviews.length; i++) {
              await weekViewAvailabilityPage.verifyAllDayCardInformationDisplayedCorrectly(
                weekCase.dayOverviews[i],
              );
            }
          });
        });
      });
    });
  });
});
