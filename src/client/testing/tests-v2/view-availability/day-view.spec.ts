/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { test, expect } from '../../fixtures-v2';
import {
  clockBackwardDaysData,
  clockForwardDaysData,
} from '../../availability';

test.describe('View Daily Availability', () => {
  ['Europe/London', 'Asia/Kamchatka', 'US/Pacific'].forEach(async timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      const forwardDayData = clockForwardDaysData();
      const backwardDayData = clockBackwardDaysData();

      const allDays = forwardDayData.dayTestCases.concat(
        backwardDayData.dayTestCases,
      );

      allDays.forEach(dayCase => {
        test.describe(`Day: '${dayCase.dayCardHeader}'`, () => {
          test('Change session has the correct information on the edit session decision page', async ({
            setup,
            page,
            changeAvailabilityPage,
          }) => {
            const { site } = await setup({
              availability: forwardDayData.availability?.concat(
                backwardDayData.availability!,
              ),
              bookings: forwardDayData.bookings?.concat(
                backwardDayData.bookings!,
              ),
            });

            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            const changeButton = page
              .getByRole('heading', {
                name: dayCase.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'Change',
              });

            await changeButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week/edit-session?date=${dayCase.day}&session**`,
            );

            await expect(changeAvailabilityPage.changeHeader).toHaveText(
              `${site.name}Change availability for ${dayCase.changeSessionHeader}`,
            );

            //table headers!
            await expect(
              page.getByRole('columnheader', {
                name: 'Time',
              }),
            ).toBeVisible();

            await expect(
              page.getByRole('columnheader', {
                name: 'Services',
              }),
            ).toBeVisible();

            await expect(
              page.getByRole('columnheader', {
                name: 'Booked',
                exact: true,
              }),
            ).toBeVisible();

            //no action header
            await expect(
              page.getByRole('columnheader', {
                name: 'Action',
              }),
            ).not.toBeVisible();

            const timeCell = page.getByRole('cell', {
              name: dayCase.timeRange,
            });
            const serviceCell = page.getByRole('cell', {
              name: dayCase.service,
            });
            const bookedCell = page.getByRole('cell', {
              name: `${dayCase.booked} booked`,
            });

            await expect(timeCell).toBeVisible();
            await expect(serviceCell).toBeVisible();
            await expect(bookedCell).toBeVisible();
          });

          test('Change session has the correct information on the edit session page', async ({
            setup,
            page,
            changeAvailabilityPage,
            addSessionPage,
          }) => {
            const { site } = await setup({
              availability: forwardDayData.availability?.concat(
                backwardDayData.availability!,
              ),
              bookings: forwardDayData.bookings?.concat(
                backwardDayData.bookings!,
              ),
            });

            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            const changeButton = page
              .getByRole('heading', {
                name: dayCase.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'Change',
              });

            await changeButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week/edit-session?date=${dayCase.day}&session**`,
            );

            expect(changeAvailabilityPage.changeHeader).toHaveText(
              `${site.name}Change availability for ${dayCase.changeSessionHeader}`,
            );

            await changeAvailabilityPage.selectChangeType(
              'ChangeLengthCapacity',
            );
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL(
              `**/site/${site.id}/availability/edit?session**`,
            );

            await expect(addSessionPage.addSessionHeader).toHaveText(
              `Edit sessionEdit time and capacity for ${dayCase.changeSessionHeader}`,
            );

            await expect(
              page.getByText(
                `${dayCase.booked} booked appointments in this session.`,
              ),
            ).toBeVisible();
            await expect(
              page.getByText(
                `${dayCase.unbooked} unbooked appointments in this session.`,
              ),
            ).toBeVisible();

            await expect(addSessionPage.startTimeHour).toHaveValue(
              dayCase.startHour,
            );
            await expect(addSessionPage.startTimeMinute).toHaveValue(
              dayCase.startMins,
            );
            await expect(addSessionPage.endTimeHour).toHaveValue(
              dayCase.endHour,
            );
            await expect(addSessionPage.endTimeMinute).toHaveValue(
              dayCase.endMins,
            );
          });

          test('Change session has the correct information on the cancel session page', async ({
            setup,
            page,
            changeAvailabilityPage,
            cancelSessionDetailsPage,
          }) => {
            const { site } = await setup({
              availability: forwardDayData.availability?.concat(
                backwardDayData.availability!,
              ),
              bookings: forwardDayData.bookings?.concat(
                backwardDayData.bookings!,
              ),
            });

            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            const changeButton = page
              .getByRole('heading', {
                name: dayCase.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'Change',
              });

            await changeButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week/edit-session?date=${dayCase.day}&session**`,
            );

            expect(changeAvailabilityPage.changeHeader).toHaveText(
              `${site.name}Change availability for ${dayCase.changeSessionHeader}`,
            );

            await changeAvailabilityPage.selectChangeType('CancelSession');
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL(
              `**/site/${site.id}/availability/cancel/confirmation?session**`,
            );

            await expect(
              cancelSessionDetailsPage.cancelSessionHeader,
            ).toHaveText(
              `${site.name}Cancel session for ${dayCase.cancelSessionHeader}`,
            );

            //single table
            await expect(page.locator('table')).toHaveCount(1);

            //table headers!
            await expect(
              page.getByRole('columnheader', {
                name: 'Time',
              }),
            ).toBeVisible();

            await expect(
              page.getByRole('columnheader', {
                name: 'Services',
              }),
            ).toBeVisible();

            //no actions
            await expect(
              page.getByRole('columnheader', {
                name: 'Action',
              }),
            ).not.toBeVisible();

            const timeCell = page.getByRole('cell', {
              name: dayCase.timeRange,
            });
            const serviceCell = page.getByRole('cell', {
              name: dayCase.service,
            });

            await expect(timeCell).toBeVisible();
            await expect(serviceCell).toBeVisible();
          });

          //TODO add back in
          test('View daily appointments has the correct information, and on the cancel appointment page', async ({
            setup,
            page,
            dailyAppointmentDetailsPage,
            cancelAppointmentDetailsPage,
          }) => {
            const { site } = await setup({
              availability: forwardDayData.availability?.concat(
                backwardDayData.availability!,
              ),
              bookings: forwardDayData.bookings?.concat(
                backwardDayData.bookings!,
              ),
            });

            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${dayCase.day}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            const viewDailyAppointmentsButton = page
              .getByRole('heading', {
                name: dayCase.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'View daily appointments',
              });

            await viewDailyAppointmentsButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/daily-appointments?date=${dayCase.day}&page=1`,
            );

            await dailyAppointmentDetailsPage.verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
              dayCase.viewDailyAppointments,
            );

            const allTableRows =
              await dailyAppointmentDetailsPage.appointmentsTable
                .getByRole('row')
                .all();

            //dive into the cancel details page and verify information is correct
            for (
              let index = 0;
              index < dayCase.cancelDailyAppointments.length;
              index++
            ) {
              const expectedAppointment =
                dayCase.cancelDailyAppointments[index];

              //start at 1 to ignore header row
              const tableRow = allTableRows[index + 1];

              const cancelLink = tableRow.getByRole('link', {
                name: 'Cancel',
              });

              await expect(cancelLink).toBeEnabled();

              await cancelLink.click();

              await page.waitForURL(`**/site/${site.id}/appointment/**/cancel`);

              await cancelAppointmentDetailsPage.verifyAppointmentDetailsDisplayed(
                expectedAppointment,
              );

              //need to go back after this check
              await cancelAppointmentDetailsPage.backButton.click();

              await page.waitForURL(
                `**/site/${site.id}/view-availability/daily-appointments?date=${dayCase.day}&page=1`,
              );
            }
          });
        });
      });
    });
  });
});
