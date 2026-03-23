import {
  OAuthLoginPage,
  RootPage,
  NotFoundPage,
  AddSessionPage,
  CheckSessionDetailsPage,
  AddServicesPage,
  CheckAnswersPage,
} from '@testing-page-objects';
import { Site } from '@types';
import { test, expect, overrideFeatureFlag, Page } from '../../fixtures';
import {
  daysFromToday,
  getDateInFuture,
  getLongDayDateText,
} from '../../utils/date-utility';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let checkAnswersPage: CheckAnswersPage;
let addSessionPage: AddSessionPage;
let addServicesPage: AddServicesPage;
let checkSessionDetailsPage: CheckSessionDetailsPage;

let site: Site;

test.describe.configure({ mode: 'serial' });

const createSessionOnDay = async (page: Page, dayIncrement: number) => {
  const dateStringForTest = daysFromToday(dayIncrement);

  //create one session
  await page.goto(
    `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${dateStringForTest}`,
  );
  await page.waitForURL(
    `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${dateStringForTest}`,
  );

  await addSessionPage.addSession('09', '00', '17', '00', '2', '5');
  await addServicesPage.addService('Flu 18 to 64');
  await checkSessionDetailsPage.saveSession();
  await page.waitForURL('**/site/**/view-availability/week?date=**');
};

[true, false].forEach(CancelADateRangeFlagEnabled => {
  test.describe(`Test with CancelADateRangeFlag: '${CancelADateRangeFlagEnabled}'`, () => {
    test.beforeAll(async () => {
      await overrideFeatureFlag(
        'CancelADateRange',
        CancelADateRangeFlagEnabled,
      );
    });

    test.afterAll(async () => {
      await overrideFeatureFlag('CancelADateRange', false);
    });

    test.describe('Cancel A Date Range', () => {
      test.beforeEach(async ({ page, getTestSite }) => {
        site = getTestSite(2);
        rootPage = new RootPage(page);
        oAuthPage = new OAuthLoginPage(page);

        await rootPage.goto();
        await rootPage.pageContentLogInButton.click();
        await oAuthPage.signIn();

        await page.waitForURL(`/manage-your-appointments/sites`);
        await page
          .getByRole('link', { name: 'View Church Lane Pharmacy' })
          .click();
        await page.waitForURL(`/manage-your-appointments/site/${site.id}`);
        await page
          .getByRole('link', { name: 'View availability and manage' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );
      });

      test('Cancel a date range monthly page', async ({ page }) => {
        const notFoundPage = new NotFoundPage(page);
        if (!CancelADateRangeFlagEnabled) {
          await page.goto(
            `/manage-your-appointments/site/${site.id}/change-availability`,
          );
          await expect(notFoundPage.title).toBeVisible();
          return;
        }

        await expect(
          page.getByRole('button', { name: 'Change availability' }),
        ).toBeVisible();
        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('link', { name: 'Back', exact: true }),
        ).toBeVisible();
        await page.getByRole('link', { name: 'Back', exact: true }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );
      });

      test('Cancel a date range weekly page', async ({ page }) => {
        const notFoundPage = new NotFoundPage(page);
        if (!CancelADateRangeFlagEnabled) {
          await page.goto(
            `/manage-your-appointments/site/${site.id}/change-availability`,
          );
          await expect(notFoundPage.title).toBeVisible();
          return;
        }

        const firstWeeklyCard = page.locator('div.nhsuk-card').first();
        await firstWeeklyCard.getByRole('link', { name: 'View week' }).click();
        await page.waitForURL(/.*\/view-availability\/week/);

        await expect(
          page.getByRole('button', { name: 'Change availability' }),
        ).toBeVisible();
        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('link', { name: 'Back', exact: true }),
        ).toBeVisible();
        await page.getByRole('link', { name: 'Back', exact: true }).click();
        await page.waitForURL(/.*\/view-availability(\/week)?/);
      });

      test('Cancel a date range daily page', async ({ page }) => {
        const notFoundPage = new NotFoundPage(page);
        if (!CancelADateRangeFlagEnabled) {
          await page.goto(
            `/manage-your-appointments/site/${site.id}/change-availability`,
          );
          await expect(notFoundPage.title).toBeVisible();
          return;
        }

        const lastWeekCard = page.locator('div.nhsuk-card').last();
        await lastWeekCard.getByRole('link', { name: 'View week' }).click();
        await page.waitForURL(/.*\/view-availability\/week/);

        const dayCards = page.locator('div.nhsuk-card');
        const cardCount = await dayCards.count();
        let linkFound = false;

        // Iterate through the day cards to find "View daily appointments" link
        for (let i = 0; i < cardCount; i++) {
          const currentCard = dayCards.nth(i);
          const viewDailyAppointmentsLink = currentCard.getByRole('link', {
            name: 'View daily appointments',
          });

          if (await viewDailyAppointmentsLink.isVisible()) {
            await Promise.all([
              page.waitForURL(
                /\/manage-your-appointments\/site\/.*\/view-availability\/daily-appointments\?date=.*/,
              ),
              viewDailyAppointmentsLink.click(),
            ]);
            linkFound = true;
            break; // Stop at the first card that matches
          }
        }

        // If no link was found, go to the last card and click "Add availability"
        if (!linkFound && cardCount > 0) {
          const lastCard = dayCards.last();
          await lastCard
            .getByRole('link', { name: 'Add availability to this day' })
            .click();
          await page.waitForURL(
            /\/manage-your-appointments\/site\/.*\/create-availability\/wizard\?date=.*/,
          );
          await page
            .getByRole('textbox', { name: 'Session start time - hour' })
            .fill('09');
          await page
            .getByRole('textbox', { name: 'Session start time - minute' })
            .fill('00');
          await page
            .getByRole('textbox', { name: 'Session end time - hour' })
            .fill('10');
          await page
            .getByRole('textbox', { name: 'Session end time - minute' })
            .fill('00');
          // if the element is an <input type="number">, Playwright will identify it as a spinbutton, and textbox may not find it.
          await page
            .getByRole('spinbutton', {
              name: 'How many vaccinators or vaccination spaces do you have?',
            })
            .fill('1');
          await page
            .getByRole('spinbutton', {
              name: 'How long are your appointments?',
            })
            .fill('10');
          await page.getByRole('button', { name: 'Continue' }).click();
          await page
            .getByRole('checkbox', { name: 'Flu and COVID 18 to 64' })
            .check();
          await page.getByRole('button', { name: 'Continue' }).click();
          await page
            .getByRole('button', { name: 'Save and publish availability' })
            .click();
          // Redirect back to the daily page
          await page.waitForURL(
            /\/manage-your-appointments\/site\/.*\/view-availability\/week\?date=.*/,
          );
          // Now that you have availability, the "View daily appointments" will appear
          const lastDayCard = page.locator('div.nhsuk-card').last();
          await Promise.all([
            page.waitForURL(
              /\/manage-your-appointments\/site\/.*\/view-availability\/daily-appointments\?date=.*/,
            ),
            lastDayCard
              .getByRole('link', { name: 'View daily appointments' })
              .click(),
          ]);
        }

        await expect(
          page.getByRole('button', { name: 'Change availability' }),
        ).toBeVisible();
        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('link', { name: 'Back', exact: true }),
        ).toBeVisible();
        await page.getByRole('link', { name: 'Back', exact: true }).click();
        await page.waitForURL(/.*\/view-availability(\/daily-appointments)?/);
      });
    });

    test.describe('Cancellation Confirmation', () => {
      test.beforeEach(async ({ page, getTestSite }) => {
        site = getTestSite(2);
        rootPage = new RootPage(page);
        oAuthPage = new OAuthLoginPage(page);
        addSessionPage = new AddSessionPage(page);
        addServicesPage = new AddServicesPage(page);
        checkSessionDetailsPage = new CheckSessionDetailsPage(page);

        await rootPage.goto();
        await rootPage.pageContentLogInButton.click();
        await oAuthPage.signIn();

        await page.goto('/manage-your-appointments/sites');
        await page.waitForURL(`/manage-your-appointments/sites`);
        await page
          .getByRole('link', { name: 'View Church Lane Pharmacy' })
          .click();
        await page.waitForURL(`/manage-your-appointments/site/${site.id}`);
      });

      test('Cancel sessions, verify sessions have been cancelled', async ({
        page,
      }) => {
        const notFoundPage = new NotFoundPage(page);
        if (!CancelADateRangeFlagEnabled) {
          await page.goto(
            `/manage-your-appointments/site/${site.id}/change-availability`,
          );
          await expect(notFoundPage.title).toBeVisible();
          return;
        }

        const fromDayIncrement = 170;
        const toDayIncrement = 171;
        const fromDate = getDateInFuture(fromDayIncrement);
        const toDate = getDateInFuture(toDayIncrement);
        await createSessionOnDay(page, fromDayIncrement);
        await createSessionOnDay(page, toDayIncrement);

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.locator('#start-date-day').fill(fromDate.day.toString());
        await page.locator('#start-date-month').fill(fromDate.month.toString());
        await page.locator('#start-date-year').fill(fromDate.year.toString());
        await page.locator('#end-date-day').fill(toDate.day.toString());
        await page.locator('#end-date-month').fill(toDate.month.toString());
        await page.locator('#end-date-year').fill(toDate.year.toString());
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: 'You are about to cancel',
          }),
        ).toBeVisible();

        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await page
          .getByRole('button', { name: 'Cancel sessions', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: /2 sessions cancelled/i,
          }),
        ).toBeVisible();

        await page
          .getByRole('link', { name: 'Go back to view availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );

        await expect(
          page.getByRole('heading', { name: /View availability/i }),
        ).toBeVisible();

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${fromDate.year}-${fromDate.month}-${fromDate.day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${fromDate.year}-${fromDate.month}-${fromDate.day}`,
        );

        const appointmentCard = page
          .locator('.nhsuk-card')
          .filter({ hasText: getLongDayDateText(fromDate) });

        await expect(appointmentCard).toBeVisible();
        const addLink = appointmentCard.getByRole('link', {
          name: 'Add availability to this day',
        });
        await expect(addLink).toBeVisible();

        expect(appointmentCard.getByText('No availability')).toBeVisible();
        await expect(
          appointmentCard.getByText('Total appointments: 0'),
        ).toBeVisible();

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${toDate.year}-${toDate.month}-${toDate.day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${toDate.year}-${toDate.month}-${toDate.day}`,
        );

        const toDateAppointmentCard = page
          .locator('.nhsuk-card')
          .filter({ hasText: getLongDayDateText(toDate) });

        await expect(toDateAppointmentCard).toBeVisible();
        await expect(
          toDateAppointmentCard.getByRole('link', {
            name: 'Add availability to this day',
          }),
        ).toBeVisible();

        expect(
          toDateAppointmentCard.getByText('No availability'),
        ).toBeVisible();
        await expect(
          toDateAppointmentCard.getByText('Total appointments: 0'),
        ).toBeVisible();
      });

      test('Cancel session, verify session has been cancelled', async ({
        page,
      }) => {
        const notFoundPage = new NotFoundPage(page);
        if (!CancelADateRangeFlagEnabled) {
          await page.goto(
            `/manage-your-appointments/site/${site.id}/change-availability`,
          );
          await expect(notFoundPage.title).toBeVisible();
          return;
        }

        const dayIncrement = 170;
        const fromDate = getDateInFuture(dayIncrement);
        const toDate = getDateInFuture(dayIncrement);
        await createSessionOnDay(page, dayIncrement);

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.locator('#start-date-day').fill(fromDate.day.toString());
        await page.locator('#start-date-month').fill(fromDate.month.toString());
        await page.locator('#start-date-year').fill(fromDate.year.toString());
        await page.locator('#end-date-day').fill(toDate.day.toString());
        await page.locator('#end-date-month').fill(toDate.month.toString());
        await page.locator('#end-date-year').fill(toDate.year.toString());
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: 'You are about to cancel',
          }),
        ).toBeVisible();

        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await page
          .getByRole('button', { name: 'Cancel sessions', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: /1 session cancelled/i,
          }),
        ).toBeVisible();

        await page
          .getByRole('link', { name: 'Go back to view availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );

        await expect(
          page.getByRole('heading', { name: /View availability/i }),
        ).toBeVisible();

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${fromDate.year}-${fromDate.month}-${fromDate.day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${toDate.year}-${toDate.month}-${toDate.day}`,
        );

        const appointmentCard = page
          .locator('.nhsuk-card')
          .filter({ hasText: getLongDayDateText(fromDate) });

        await expect(appointmentCard).toBeVisible();
        const addLink = appointmentCard.getByRole('link', {
          name: 'Add availability to this day',
        });
        await expect(addLink).toBeVisible();

        expect(appointmentCard.getByText('No availability')).toBeVisible();
        await expect(
          appointmentCard.getByText('Total appointments: 0'),
        ).toBeVisible();
      });
    });
  });
});
