import {
  OAuthLoginPage,
  RootPage,
  AddSessionPage,
  CancelSessionDetailsPage,
  ChangeAvailabilityPage,
  DailyAppointmentDetailsPage,
  WeekViewAvailabilityPage,
  CancelAppointmentDetailsPage,
} from '@testing-page-objects';
import { test, expect, BrowserContext, Page } from '../../fixtures';
import { Site } from '@types';
import { sessionTestCases, weekTestCases } from '../../availability';

test.describe.skip('View Week Availability', () => {
  let rootPage: RootPage;
  let oAuthPage: OAuthLoginPage;
  let weekViewAvailabilityPage: WeekViewAvailabilityPage;
  let changeAvailabilityPage: ChangeAvailabilityPage;
  let addSessionPage: AddSessionPage;
  let cancelSessionDetailsPage: CancelSessionDetailsPage;
  let dailyAppointmentDetailsPage: DailyAppointmentDetailsPage;
  let cancelAppointmentDetailsPage: CancelAppointmentDetailsPage;
  let site: Site;

  let context: BrowserContext;
  let page: Page;

  test.beforeAll(async ({ browser, getTestSite }) => {
    context = await browser.newContext();
    page = await context.newPage();

    site = getTestSite(2);
    rootPage = new RootPage(page);
    oAuthPage = new OAuthLoginPage(page);
    weekViewAvailabilityPage = new WeekViewAvailabilityPage(page);
    changeAvailabilityPage = new ChangeAvailabilityPage(page);
    addSessionPage = new AddSessionPage(page);
    cancelSessionDetailsPage = new CancelSessionDetailsPage(page);
    dailyAppointmentDetailsPage = new DailyAppointmentDetailsPage(page);
    cancelAppointmentDetailsPage = new CancelAppointmentDetailsPage(page);

    await rootPage.goto();
    await rootPage.pageContentLogInButton.click();
    await oAuthPage.signIn();
  });

  test.afterAll(async () => {
    await context.close();
  });

  // ['Europe/London', 'UTC', 'Pacific/Kiritimati', 'Etc/GMT+12']
  ['Europe/London'].forEach(timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      weekTestCases.forEach(week => {
        test.describe(`Session tests for week: '${week.weekHeader}'`, () => {
          test.beforeEach(async () => {
            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${week.week}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${week.week}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });
          });

          test(`View week page data is arranged in the day cards as expected`, async () => {
            await weekViewAvailabilityPage.verifyViewNextAndPreviousWeeksButtonsDisplayed(
              week.previousWeek,
              week.nextWeek,
            );

            for (let i = 0; i < week.dayOverviews.length; i++) {
              await weekViewAvailabilityPage.verifyAllDayCardInformationDisplayedCorrectly(
                week.dayOverviews[i],
              );
            }
          });
        });
      });

      sessionTestCases.forEach(daySession => {
        test.describe(`Session tests for day: '${daySession.dayCardHeader}'`, () => {
          test.beforeEach(async () => {
            //start test by navigating to the week view that contains this session
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability/week?date=${daySession.week}`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability/week?date=${daySession.week}`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });
          });

          test('Change session has the correct information on the edit session decision page', async () => {
            const changeButton = page
              .getByRole('heading', {
                name: daySession.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'Change',
              });

            await changeButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
            );

            expect(changeAvailabilityPage.changeHeader).toHaveText(
              `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
            );

            //table headers!
            await expect(
              changeAvailabilityPage.page.getByRole('columnheader', {
                name: 'Time',
              }),
            ).toBeVisible();

            await expect(
              changeAvailabilityPage.page.getByRole('columnheader', {
                name: 'Services',
              }),
            ).toBeVisible();

            await expect(
              changeAvailabilityPage.page.getByRole('columnheader', {
                name: 'Booked',
                exact: true,
              }),
            ).toBeVisible();

            //no action header
            await expect(
              changeAvailabilityPage.page.getByRole('columnheader', {
                name: 'Action',
              }),
            ).not.toBeVisible();

            const timeCell = changeAvailabilityPage.page.getByRole('cell', {
              name: daySession.timeRange,
            });
            const serviceCell = changeAvailabilityPage.page.getByRole('cell', {
              name: daySession.service,
            });
            const bookedCell = changeAvailabilityPage.page.getByRole('cell', {
              name: `${daySession.booked} booked`,
            });

            await expect(timeCell).toBeVisible();
            await expect(serviceCell).toBeVisible();
            await expect(bookedCell).toBeVisible();
          });

          test('Change session has the correct information on the edit session page', async () => {
            const changeButton = page
              .getByRole('heading', {
                name: daySession.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'Change',
              });

            await changeButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
            );

            expect(changeAvailabilityPage.changeHeader).toHaveText(
              `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
            );

            await changeAvailabilityPage.selectChangeType(
              'ChangeLengthCapacity',
            );
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL(
              `**/site/${site.id}/availability/edit?session**`,
            );

            await expect(addSessionPage.addSessionHeader).toHaveText(
              `Edit sessionEdit time and capacity for ${daySession.changeSessionHeader}`,
            );

            await expect(
              addSessionPage.page.getByText(
                `${daySession.booked} booked appointments in this session.`,
              ),
            ).toBeVisible();
            await expect(
              addSessionPage.page.getByText(
                `${daySession.unbooked} unbooked appointments in this session.`,
              ),
            ).toBeVisible();

            await expect(addSessionPage.startTimeHour).toHaveValue(
              daySession.startHour,
            );
            await expect(addSessionPage.startTimeMinute).toHaveValue(
              daySession.startMins,
            );
            await expect(addSessionPage.endTimeHour).toHaveValue(
              daySession.endHour,
            );
            await expect(addSessionPage.endTimeMinute).toHaveValue(
              daySession.endMins,
            );
          });

          test('Change session has the correct information on the cancel session page', async () => {
            const changeButton = page
              .getByRole('heading', {
                name: daySession.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'Change',
              });

            await changeButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
            );

            expect(changeAvailabilityPage.changeHeader).toHaveText(
              `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
            );

            await changeAvailabilityPage.selectChangeType('CancelSession');
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL(
              `**/site/${site.id}/availability/cancel/confirmation?session**`,
            );

            await expect(
              cancelSessionDetailsPage.cancelSessionHeader,
            ).toHaveText(
              `${site.name}Cancel session for ${daySession.dayCardHeader}`,
            );

            //single table
            await expect(
              cancelSessionDetailsPage.page.locator('table'),
            ).toHaveCount(1);

            //table headers!
            await expect(
              cancelSessionDetailsPage.page.getByRole('columnheader', {
                name: 'Time',
              }),
            ).toBeVisible();

            await expect(
              cancelSessionDetailsPage.page.getByRole('columnheader', {
                name: 'Services',
              }),
            ).toBeVisible();

            //no actions
            await expect(
              cancelSessionDetailsPage.page.getByRole('columnheader', {
                name: 'Action',
              }),
            ).not.toBeVisible();

            const timeCell = cancelSessionDetailsPage.page.getByRole('cell', {
              name: daySession.timeRange,
            });
            const serviceCell = cancelSessionDetailsPage.page.getByRole(
              'cell',
              {
                name: daySession.service,
              },
            );

            await expect(timeCell).toBeVisible();
            await expect(serviceCell).toBeVisible();
          });

          test('View daily appointments has the correct information, and on the cancel appointment page', async () => {
            const viewDailyAppointmentsButton = page
              .getByRole('heading', {
                name: daySession.dayCardHeader,
              })
              .locator('../..')
              .getByRole('link', {
                name: 'View daily appointments',
              });

            await viewDailyAppointmentsButton.click();

            await page.waitForURL(
              `**/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
            );

            await dailyAppointmentDetailsPage.verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
              daySession.viewDailyAppointments,
            );

            const allTableRows =
              await dailyAppointmentDetailsPage.appointmentsTable
                .getByRole('row')
                .all();

            //dive into the cancel details page and verify information is correct
            for (
              let index = 0;
              index < daySession.cancelDailyAppointments.length;
              index++
            ) {
              const expectedAppointment =
                daySession.cancelDailyAppointments[index];

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
                `**/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
              );
            }
          });
        });
      });
    });
  });
});
