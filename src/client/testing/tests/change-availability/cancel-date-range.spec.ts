import { OAuthLoginPage, RootPage, NotFoundPage } from '@testing-page-objects';
import { Site } from '@types';
import { test, expect, overrideFeatureFlag } from '../../fixtures';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;

let site: Site;

test.describe.configure({ mode: 'serial' });

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

    test.describe('Select Dates To Cancel', () => {
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

      test('Select dates to cancel error, mandatory field validation', async ({
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

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await page.getByRole('button', { name: 'Continue' }).click();

        // Use locator that finds the error text specifically
        const startDateError = page
          .locator('.nhsuk-form-group')
          .filter({ hasText: 'Start date' })
          .locator('.nhsuk-error-message');
        await expect(startDateError).toContainText('Enter a start date');

        const endDateError = page
          .locator('.nhsuk-form-group')
          .filter({ hasText: 'End date' })
          .locator('.nhsuk-error-message');
        await expect(endDateError).toContainText('Enter an end date');

        await expect(
          page.locator('.nhsuk-u-visually-hidden').first(),
        ).toHaveText('Error: ');

        await expect(
          page.getByRole('link', { name: 'Back', exact: true }),
        ).toBeVisible();
        await page.getByRole('link', { name: 'Back', exact: true }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
      });

      test('Select dates to cancel error, must be in the future', async ({
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

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        // Calculate yesterday
        const yesterday = new Date();
        yesterday.setDate(yesterday.getDate() - 1);

        const day = yesterday.getDate().toString();
        const month = (yesterday.getMonth() + 1).toString();
        const year = yesterday.getFullYear().toString();

        // Fill start date (Past)
        await page.locator('#start-date-day').fill(day);
        await page.locator('#start-date-month').fill(month);
        await page.locator('#start-date-year').fill(year);

        // Fill end date (Past)
        await page.locator('#end-date-day').fill(day);
        await page.locator('#end-date-month').fill(month);
        await page.locator('#end-date-year').fill(year);

        // Trigger Validation
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();

        // Assert both errors appear
        // We check for the specific error text your app generates for past dates
        const errorMessages = page.locator('.nhsuk-error-message');

        await expect(
          errorMessages.filter({ hasText: /Start date/ }),
        ).toContainText(/must be in the future/i);

        await expect(
          errorMessages.filter({ hasText: /End date/ }),
        ).toContainText(/must be in the future/i);

        await expect(
          page.getByRole('link', { name: 'Back', exact: true }),
        ).toBeVisible();
        await page.getByRole('link', { name: 'Back', exact: true }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
      });

      test('Select dates to cancel error, end date must be after the start date', async ({
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

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        // Start is 5 days from now, End is 2 days from now
        const now = new Date();

        const startDate = new Date(now);
        startDate.setDate(now.getDate() + 5);

        const endDate = new Date(now);
        endDate.setDate(now.getDate() + 2);

        // Fill Start Date (The later date)
        await page
          .locator('#start-date-day')
          .fill(startDate.getDate().toString());
        await page
          .locator('#start-date-month')
          .fill((startDate.getMonth() + 1).toString());
        await page
          .locator('#start-date-year')
          .fill(startDate.getFullYear().toString());

        // Fill End Date (The earlier date - INVALID)
        await page.locator('#end-date-day').fill(endDate.getDate().toString());
        await page
          .locator('#end-date-month')
          .fill((endDate.getMonth() + 1).toString());
        await page
          .locator('#end-date-year')
          .fill(endDate.getFullYear().toString());

        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();

        // Look for the specific sequence error
        const errorContainer = page
          .locator('.nhsuk-form-group--error')
          .filter({ hasText: 'End date' });
        await expect(
          errorContainer.locator('.nhsuk-error-message'),
        ).toContainText(/End date must be on or after the start date/i);

        await expect(
          page.getByRole('link', { name: 'Back', exact: true }),
        ).toBeVisible();
        await page.getByRole('link', { name: 'Back', exact: true }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
      });

      test('Select dates to cancel error within 3 months - 90 days or less', async ({
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

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        const now = new Date();

        const startDate = new Date(now);
        startDate.setDate(now.getDate() + 1);

        const endDate = new Date(now);
        // 90 days will pass validation
        endDate.setDate(now.getDate() + 90);

        // Fill Start Date (Tomorrow)
        await page
          .locator('#start-date-day')
          .fill(startDate.getDate().toString());
        await page
          .locator('#start-date-month')
          .fill((startDate.getMonth() + 1).toString());
        await page
          .locator('#start-date-year')
          .fill(startDate.getFullYear().toString());

        // Fill End Date (+90 days)
        await page.locator('#end-date-day').fill(endDate.getDate().toString());
        await page
          .locator('#end-date-month')
          .fill((endDate.getMonth() + 1).toString());
        await page
          .locator('#end-date-year')
          .fill(endDate.getFullYear().toString());

        // Trigger the validation
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
      });

      test('Select dates to cancel error within 3 months - greater than 90 days', async ({
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

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        const now = new Date();

        const startDate = new Date(now);
        startDate.setDate(now.getDate() + 1);

        const endDate = new Date(now);
        // Greater than 90 days will fail validation
        endDate.setDate(now.getDate() + 92);

        // Fill Start Date (Tomorrow)
        await page
          .locator('#start-date-day')
          .fill(startDate.getDate().toString());
        await page
          .locator('#start-date-month')
          .fill((startDate.getMonth() + 1).toString());
        await page
          .locator('#start-date-year')
          .fill(startDate.getFullYear().toString());

        // Fill End Date (+91 days)
        await page.locator('#end-date-day').fill(endDate.getDate().toString());
        await page
          .locator('#end-date-month')
          .fill((endDate.getMonth() + 1).toString());
        await page
          .locator('#end-date-year')
          .fill(endDate.getFullYear().toString());

        // Trigger the validation
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();

        // Target the Start Date group by its specific legend
        const startDateGroup = page.locator('.nhsuk-form-group').filter({
          has: page.locator('legend').getByText('Start date', { exact: true }),
        });

        await expect(
          startDateGroup.locator('.nhsuk-error-message'),
        ).toContainText('Start date must be');

        // Target the End Date group by its specific legend
        const endDateGroup = page.locator('.nhsuk-form-group').filter({
          has: page.locator('legend').getByText('End date', { exact: true }),
        });

        await expect(
          endDateGroup.locator('.nhsuk-error-message'),
        ).toContainText('End date must be');

        await expect(
          page.getByRole('link', { name: 'Back', exact: true }),
        ).toBeVisible();
        await page.getByRole('link', { name: 'Back', exact: true }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
      });
    });

    test.describe('Cannot Cancel', () => {
      test.beforeEach(async ({ page, getTestSite }) => {
        site = getTestSite(2);
        rootPage = new RootPage(page);
        oAuthPage = new OAuthLoginPage(page);

        await rootPage.goto();
        await rootPage.pageContentLogInButton.click();
        await oAuthPage.signIn();

        await page.goto('/manage-your-appointments/sites');
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

      test('Cannot cancel these sessions - Return to view availability', async ({
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

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('button', { name: 'Continue to cancel' }),
        ).toBeVisible();
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        const now = new Date();
        const startDate = new Date(now);
        startDate.setDate(now.getDate() + 1);
        const endDate = new Date(now);
        endDate.setDate(now.getDate() + 23);

        await page
          .locator('#start-date-day')
          .fill(startDate.getDate().toString());
        await page
          .locator('#start-date-month')
          .fill((startDate.getMonth() + 1).toString());
        await page
          .locator('#start-date-year')
          .fill(startDate.getFullYear().toString());
        await page.locator('#end-date-day').fill(endDate.getDate().toString());
        await page
          .locator('#end-date-month')
          .fill((endDate.getMonth() + 1).toString());
        await page
          .locator('#end-date-year')
          .fill(endDate.getFullYear().toString());
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page
          .getByRole('button', { name: 'Return to view availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );
      });

      test('Cannot cancel these sessions - Select different dates', async ({
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

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('button', { name: 'Continue to cancel' }),
        ).toBeVisible();
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        const now = new Date();
        const startDate = new Date(now);
        startDate.setDate(now.getDate() + 1);
        const endDate = new Date(now);
        endDate.setDate(now.getDate() + 23);

        await page
          .locator('#start-date-day')
          .fill(startDate.getDate().toString());
        await page
          .locator('#start-date-month')
          .fill((startDate.getMonth() + 1).toString());
        await page
          .locator('#start-date-year')
          .fill(startDate.getFullYear().toString());
        await page.locator('#end-date-day').fill(endDate.getDate().toString());
        await page
          .locator('#end-date-month')
          .fill((endDate.getMonth() + 1).toString());
        await page
          .locator('#end-date-year')
          .fill(endDate.getFullYear().toString());
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await expect(
          page.getByRole('button', {
            name: 'Select different dates',
            exact: true,
          }),
        ).toBeVisible();
        await page
          .getByRole('button', { name: 'Select different dates' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('heading', { name: 'Select dates to cancel' }),
        ).toBeVisible();

        // Verify that the inputs still contain the dates previously populated
        await expect(page.locator('#start-date-day')).toHaveValue(
          startDate.getDate().toString(),
        );
        await expect(page.locator('#start-date-month')).toHaveValue(
          (startDate.getMonth() + 1).toString(),
        );
        await expect(page.locator('#start-date-year')).toHaveValue(
          startDate.getFullYear().toString(),
        );
        await expect(page.locator('#end-date-day')).toHaveValue(
          endDate.getDate().toString(),
        );
        await expect(page.locator('#end-date-month')).toHaveValue(
          (endDate.getMonth() + 1).toString(),
        );
        await expect(page.locator('#end-date-year')).toHaveValue(
          endDate.getFullYear().toString(),
        );
      });
    });
  });
});

//TODO do we need to assert any cases where CancelADateRangeFlag is disabled here??
//for now, assume CancelADateRange is enabled for each child flag test
[true, false].forEach(CancelADateRangeWithBookingsFlagEnabled => {
  test.describe(`Test with CancelADateRangeWithBookingsFlag: '${CancelADateRangeWithBookingsFlagEnabled}'`, () => {
    test.beforeAll(async () => {
      await overrideFeatureFlag('CancelADateRange', true);
      await overrideFeatureFlag(
        'CancelADateRangeWithBookings',
        CancelADateRangeWithBookingsFlagEnabled,
      );
    });

    test.afterAll(async () => {
      await overrideFeatureFlag('CancelADateRange', false);
      await overrideFeatureFlag('CancelADateRangeWithBookings', false);
    });

    test.beforeEach(async ({ page, getTestSite }) => {
      site = getTestSite(2);
      rootPage = new RootPage(page);
      oAuthPage = new OAuthLoginPage(page);

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

    test('Verify number of steps when bookings exist', async ({ page }) => {
      await page
        .getByRole('link', { name: 'View availability and manage' })
        .click();
      await page.waitForURL(
        `/manage-your-appointments/site/${site.id}/view-availability`,
      );
      await expect(
        page.getByRole('button', { name: 'Change availability' }),
      ).toBeVisible();
      await page.getByRole('button', { name: 'Change availability' }).click();
      await page.waitForURL(
        `/manage-your-appointments/site/${site.id}/change-availability`,
      );
      const listItems = page
        .locator('ol li')
        .filter({ hasNot: page.getByRole('button') });

      //happy vs sad path
      if (CancelADateRangeWithBookingsFlagEnabled) {
        // Confirming 3 items in the list
        await expect(listItems).toHaveText([
          'Cancel the sessions you want to change',
          'Choose to keep existing bookings',
          'Create new sessions with the updated details',
        ]);
      } else {
        // Confirming 2 items in the list
        await expect(listItems).toHaveText([
          'Cancel the sessions you want to change',
          'Create new sessions with the updated details',
        ]);
      }
    });

    test('There are no sessions in this date range - Choose a new date range', async ({
      page,
    }) => {
      await page
        .getByRole('link', { name: 'View availability and manage' })
        .click();
      await page.waitForURL(
        `/manage-your-appointments/site/${site.id}/view-availability`,
      );
      await expect(
        page.getByRole('button', { name: 'Change availability' }),
      ).toBeVisible();
      await page.getByRole('button', { name: 'Change availability' }).click();
      await page.waitForURL(
        `/manage-your-appointments/site/${site.id}/change-availability`,
      );
      await page.getByRole('button', { name: 'Continue to cancel' }).click();
      await page.waitForURL(
        `/manage-your-appointments/site/${site.id}/change-availability`,
      );

      const now = new Date();
      const startDate = new Date(now);
      startDate.setDate(now.getDate() + 365);
      const endDate = new Date(now);
      endDate.setDate(now.getDate() + 366);

      await page
        .locator('#start-date-day')
        .fill(startDate.getDate().toString());
      await page
        .locator('#start-date-month')
        .fill((startDate.getMonth() + 1).toString());
      await page
        .locator('#start-date-year')
        .fill(startDate.getFullYear().toString());
      await page.locator('#end-date-day').fill(endDate.getDate().toString());
      await page
        .locator('#end-date-month')
        .fill((endDate.getMonth() + 1).toString());
      await page
        .locator('#end-date-year')
        .fill(endDate.getFullYear().toString());
      await page.getByRole('button', { name: 'Continue', exact: true }).click();
      await page.waitForURL(
        `/manage-your-appointments/site/${site.id}/change-availability`,
      );
      await expect(
        page.getByRole('heading', {
          name: 'There are no sessions in this date range',
        }),
      ).toBeVisible();
      await expect(
        page.getByRole('button', { name: 'Choose a new date range' }),
      ).toBeVisible();
      await page
        .getByRole('button', { name: 'Choose a new date range' })
        .click();

      // Verify that the inputs still contain the dates previously populated
      await expect(page.locator('#start-date-day')).toHaveValue(
        startDate.getDate().toString(),
      );
      await expect(page.locator('#start-date-month')).toHaveValue(
        (startDate.getMonth() + 1).toString(),
      );
      await expect(page.locator('#start-date-year')).toHaveValue(
        startDate.getFullYear().toString(),
      );

      await expect(page.locator('#end-date-day')).toHaveValue(
        endDate.getDate().toString(),
      );
      await expect(page.locator('#end-date-month')).toHaveValue(
        (endDate.getMonth() + 1).toString(),
      );
      await expect(page.locator('#end-date-year')).toHaveValue(
        endDate.getFullYear().toString(),
      );
    });

    test('You are about to cancel X sessions - with/without bookings', async ({
      page,
    }) => {
      const now = new Date();
      const startDate = new Date(now);
      const endDate = new Date(now);

      if (CancelADateRangeWithBookingsFlagEnabled) {
        startDate.setDate(now.getDate() + 1);
        endDate.setDate(now.getDate() + 48);

        await page
          .getByRole('link', { name: 'View availability and manage' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );
        await expect(
          page.getByRole('button', { name: 'Change availability' }),
        ).toBeVisible();
        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page
          .locator('#start-date-day')
          .fill(startDate.getDate().toString());
        await page
          .locator('#start-date-month')
          .fill((startDate.getMonth() + 1).toString());
        await page
          .locator('#start-date-year')
          .fill(startDate.getFullYear().toString());
        await page.locator('#end-date-day').fill(endDate.getDate().toString());
        await page
          .locator('#end-date-month')
          .fill((endDate.getMonth() + 1).toString());
        await page
          .locator('#end-date-year')
          .fill(endDate.getFullYear().toString());
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('heading', {
            name: /You are about to cancel \d+ sessions?/i,
          }),
        ).toBeVisible();
        await expect(
          page.getByRole('radio', { name: 'Keep bookings' }),
        ).toBeVisible();
        await expect(
          page.getByRole('radio', { name: 'Cancel bookings' }),
        ).toBeVisible();
      } else {
        startDate.setDate(now.getDate() + 180);
        const startDay = startDate.getDate().toString();
        const startMonth = (startDate.getMonth() + 1).toString();
        const startYear = startDate.getFullYear().toString();
        endDate.setDate(now.getDate() + 181);
        const endDay = endDate.getDate().toString();
        const endMonth = (endDate.getMonth() + 1).toString();
        const endYear = endDate.getFullYear().toString();

        await page
          .getByLabel('Menu')
          .getByRole('link', { name: 'Create availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/create-availability`,
        );
        await page
          .getByRole('button', { name: 'Create new availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/create-availability/wizard`,
        );
        await page.getByText('Single date session', { exact: true }).click();
        await page.getByRole('button', { name: 'Continue' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/create-availability/wizard`,
        );
        await page.getByRole('textbox', { name: 'Day' }).fill(startDay);
        await page.getByRole('textbox', { name: 'Month' }).fill(startMonth);
        await page.getByRole('textbox', { name: 'Year' }).fill(startYear);
        await page.getByRole('button', { name: 'Continue' }).click();
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
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/create-availability`,
        );
        await page
          .getByLabel('Menu')
          .getByRole('link', { name: 'View availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );
        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.locator('#start-date-day').fill(startDay);
        await page.locator('#start-date-month').fill(startMonth);
        await page.locator('#start-date-year').fill(startYear);
        await page.locator('#end-date-day').fill(endDay);
        await page.locator('#end-date-month').fill(endMonth);
        await page.locator('#end-date-year').fill(endYear);
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await expect(
          page.getByRole('heading', {
            name: /You are about to cancel \d+ sessions?/i,
          }),
        ).toBeVisible();
        await expect(
          page.getByText(/There are no bookings for (this|these) sessions?/i),
        ).toBeVisible();
      }
    });
  });
});
