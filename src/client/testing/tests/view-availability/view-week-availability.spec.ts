import { WeekViewPage, LoginPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import { sessionTestCases, weekTestCases } from './view-week-availability.data';
import verifyAllDayCardInformationDisplayedCorrectly from './view-week-availability.methods';

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(
      `Test in timezone: '${timezone}'`,
      { tag: [`@timezone-test-${timezone}`] },
      () => {
        test.use({ timezoneId: timezone });

        test.describe('View Week Availability', () => {
          weekTestCases.forEach(week => {
            test.describe(`Session tests for week: '${week.weekHeader}'`, () => {
              test(`View week page data is arranged in the day cards as expected`, async ({
                signInToSite,
              }) => {
                await signInToSite(1, 2)
                  .then(sitePage => sitePage.clickViewAvailabilityCard())
                  .then(async monthViewPage => {
                    await monthViewPage.goToSpecificDate(week.week);
                    return monthViewPage.clickWeekCard(week.weekHeader);
                  })
                  .then(async weekViewPage => {
                    await expect(
                      weekViewPage.previousWeekButton.getByText(
                        week.previousWeek,
                      ),
                    ).toBeVisible();
                    await expect(
                      weekViewPage.nextWeekButton.getByText(week.nextWeek),
                    ).toBeVisible();

                    await verifyAllDayCardInformationDisplayedCorrectly(
                      weekViewPage,
                      week.dayOverviews,
                    );
                  });
              });
            });
          });

          //   sessionTestCases.forEach(daySession => {
          //     test.describe(`Session tests for day: '${daySession.dayCardHeader}'`, () => {
          //       test.beforeEach(async ({ page }) => {
          //         //start test by navigating to the week view that contains this session
          //         await page.goto(
          //           `manage-your-appointments/site/${site.id}/view-availability/week?date=${daySession.week}`,
          //         );
          //         await page.waitForURL(
          //           `**/site/${site.id}/view-availability/week?date=${daySession.week}`,
          //         );
          //         await page.waitForSelector('.nhsuk-loader', {
          //           state: 'detached',
          //         });
          //       });

          //       test('Change session has the correct information on the edit session decision page', async ({
          //         page,
          //       }) => {
          //         const changeButton = page
          //           .getByRole('heading', {
          //             name: daySession.dayCardHeader,
          //           })
          //           .locator('..')
          //           .getByRole('link', {
          //             name: 'Change',
          //           });

          //         await changeButton.click();

          //         await page.waitForURL(
          //           `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
          //         );

          //         expect(changeAvailabilityPage.changeHeader).toHaveText(
          //           `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
          //         );

          //         //table headers!
          //         await expect(
          //           changeAvailabilityPage.page.getByRole('columnheader', {
          //             name: 'Time',
          //           }),
          //         ).toBeVisible();

          //         await expect(
          //           changeAvailabilityPage.page.getByRole('columnheader', {
          //             name: 'Services',
          //           }),
          //         ).toBeVisible();

          //         await expect(
          //           changeAvailabilityPage.page.getByRole('columnheader', {
          //             name: 'Booked',
          //             exact: true,
          //           }),
          //         ).toBeVisible();

          //         await expect(
          //           changeAvailabilityPage.page.getByRole('columnheader', {
          //             name: 'Unbooked',
          //             exact: true,
          //           }),
          //         ).toBeVisible();

          //         //no action header
          //         await expect(
          //           changeAvailabilityPage.page.getByRole('columnheader', {
          //             name: 'Action',
          //           }),
          //         ).not.toBeVisible();

          //         const timeCell = changeAvailabilityPage.page.getByRole('cell', {
          //           name: daySession.timeRange,
          //         });
          //         const serviceCell = changeAvailabilityPage.page.getByRole(
          //           'cell',
          //           {
          //             name: daySession.service,
          //           },
          //         );
          //         const bookedCell = changeAvailabilityPage.page.getByRole(
          //           'cell',
          //           {
          //             name: `${daySession.booked} booked`,
          //           },
          //         );
          //         const unbookedCell = changeAvailabilityPage.page.getByRole(
          //           'cell',
          //           {
          //             name: `${daySession.unbooked} unbooked`,
          //           },
          //         );

          //         await expect(timeCell).toBeVisible();
          //         await expect(serviceCell).toBeVisible();
          //         await expect(bookedCell).toBeVisible();
          //         await expect(unbookedCell).toBeVisible();
          //       });

          //       test('Change session has the correct information on the edit session page', async ({
          //         page,
          //       }) => {
          //         const changeButton = page
          //           .getByRole('heading', {
          //             name: daySession.dayCardHeader,
          //           })
          //           .locator('..')
          //           .getByRole('link', {
          //             name: 'Change',
          //           });

          //         await changeButton.click();

          //         await page.waitForURL(
          //           `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
          //         );

          //         expect(changeAvailabilityPage.changeHeader).toHaveText(
          //           `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
          //         );

          //         await changeAvailabilityPage.selectChangeType(
          //           'ChangeLengthCapacity',
          //         );
          //         await changeAvailabilityPage.saveChanges();

          //         await page.waitForURL(
          //           `**/site/${site.id}/availability/edit?session**`,
          //         );

          //         await expect(addSessionPage.addSessionHeader).toHaveText(
          //           `Edit sessionEdit time and capacity for ${daySession.changeSessionHeader}`,
          //         );

          //         await expect(
          //           addSessionPage.page.getByText(
          //             `${daySession.booked} booked appointments in this session.`,
          //           ),
          //         ).toBeVisible();
          //         await expect(
          //           addSessionPage.page.getByText(
          //             `${daySession.unbooked} unbooked appointments in this session.`,
          //           ),
          //         ).toBeVisible();

          //         await expect(addSessionPage.startTimeHour).toHaveValue(
          //           daySession.startHour,
          //         );
          //         await expect(addSessionPage.startTimeMinute).toHaveValue(
          //           daySession.startMins,
          //         );
          //         await expect(addSessionPage.endTimeHour).toHaveValue(
          //           daySession.endHour,
          //         );
          //         await expect(addSessionPage.endTimeMinute).toHaveValue(
          //           daySession.endMins,
          //         );
          //       });

          //       test('Change session has the correct information on the cancel session page', async ({
          //         page,
          //       }) => {
          //         const changeButton = page
          //           .getByRole('heading', {
          //             name: daySession.dayCardHeader,
          //           })
          //           .locator('..')
          //           .getByRole('link', {
          //             name: 'Change',
          //           });

          //         await changeButton.click();

          //         await page.waitForURL(
          //           `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
          //         );

          //         expect(changeAvailabilityPage.changeHeader).toHaveText(
          //           `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
          //         );

          //         await changeAvailabilityPage.selectChangeType('CancelSession');
          //         await changeAvailabilityPage.saveChanges();

          //         await page.waitForURL(
          //           `**/site/${site.id}/availability/cancel?session**`,
          //         );

          //         await expect(cancelSessionPage.cancelSessionHeader).toHaveText(
          //           'Cancel sessionAre you sure you want to cancel this session?',
          //         );

          //         //single table
          //         await expect(
          //           cancelSessionPage.page.locator('table'),
          //         ).toHaveCount(1);

          //         //table headers!
          //         await expect(
          //           cancelSessionPage.page.getByRole('columnheader', {
          //             name: 'Time',
          //           }),
          //         ).toBeVisible();

          //         await expect(
          //           cancelSessionPage.page.getByRole('columnheader', {
          //             name: 'Services',
          //           }),
          //         ).toBeVisible();

          //         await expect(
          //           cancelSessionPage.page.getByRole('columnheader', {
          //             name: 'Booked',
          //             exact: true,
          //           }),
          //         ).toBeVisible();

          //         await expect(
          //           cancelSessionPage.page.getByRole('columnheader', {
          //             name: 'Unbooked',
          //             exact: true,
          //           }),
          //         ).toBeVisible();

          //         //no actions
          //         await expect(
          //           cancelSessionPage.page.getByRole('columnheader', {
          //             name: 'Action',
          //           }),
          //         ).not.toBeVisible();

          //         const timeCell = cancelSessionPage.page.getByRole('cell', {
          //           name: daySession.timeRange,
          //         });
          //         const serviceCell = cancelSessionPage.page.getByRole('cell', {
          //           name: daySession.service,
          //         });
          //         const bookedCell = cancelSessionPage.page.getByRole('cell', {
          //           name: `${daySession.booked} booked`,
          //         });
          //         const unbookedCell = cancelSessionPage.page.getByRole('cell', {
          //           name: `${daySession.unbooked} unbooked`,
          //         });

          //         await expect(timeCell).toBeVisible();
          //         await expect(serviceCell).toBeVisible();
          //         await expect(bookedCell).toBeVisible();
          //         await expect(unbookedCell).toBeVisible();
          //       });

          //       test('View daily appointments has the correct information, and on the cancel appointment page', async ({
          //         page,
          //       }) => {
          //         const viewDailyAppointmentsButton = page
          //           .getByRole('heading', {
          //             name: daySession.dayCardHeader,
          //           })
          //           .locator('..')
          //           .getByRole('link', {
          //             name: 'View daily appointments',
          //           });

          //         await viewDailyAppointmentsButton.click();

          //         await page.waitForURL(
          //           `**/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
          //         );

          //         await dailyAppointmentDetailsPage.verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
          //           daySession.viewDailyAppointments,
          //         );

          //         const allTableRows =
          //           await dailyAppointmentDetailsPage.appointmentsTable
          //             .getByRole('row')
          //             .all();

          //         //dive into the cancel details page and verify information is correct
          //         for (
          //           let index = 0;
          //           index < daySession.cancelDailyAppointments.length;
          //           index++
          //         ) {
          //           const expectedAppointment =
          //             daySession.cancelDailyAppointments[index];

          //           //start at 1 to ignore header row
          //           const tableRow = allTableRows[index + 1];

          //           const cancelLink = tableRow.getByRole('link', {
          //             name: 'Cancel',
          //           });

          //           await expect(cancelLink).toBeEnabled();

          //           await cancelLink.click();

          //           await page.waitForURL(
          //             `**/site/${site.id}/appointment/**/cancel`,
          //           );

          //           await cancelAppointmentDetailsPage.verifyAppointmentDetailsDisplayed(
          //             expectedAppointment,
          //           );

          //           //need to go back after this check
          //           await cancelAppointmentDetailsPage.backButton.click();

          //           await page.waitForURL(
          //             `**/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
          //           );
          //         }
          //       });
          //     });
          //   });
        });
      },
    );
  },
);
