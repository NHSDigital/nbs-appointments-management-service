import { OAuthLoginPage, RootPage } from '@testing-page-objects';
import { Site } from '@types';
import { test, expect, overrideFeatureFlag } from '../../fixtures';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;

let site: Site;

test.describe.configure({ mode: 'serial' });

test.beforeAll(async () => {
  await overrideFeatureFlag('CancelADateRange', true);
});

test.afterAll(async () => {
  await overrideFeatureFlag('CancelADateRange', false);
  await overrideFeatureFlag('CancelADateRangeWithBookings', true);
});

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(1);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
});

test('Select dates to cancel error, mandatory field validation', async ({
  page,
}) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );

  await page.getByRole('button', { name: 'Continue to cancel' }).click();
  await page.getByRole('button', { name: 'Continue' }).click();

  // Use locator that finds the error text specifically
  const startDateError = page
    .locator('.nhsuk-form-group')
    .filter({ hasText: 'Start date' })
    .locator('.nhsuk-error-message');
  await expect(startDateError).toContainText('Enter a start date', {
    timeout: 15000,
  });

  const endDateError = page
    .locator('.nhsuk-form-group')
    .filter({ hasText: 'End date' })
    .locator('.nhsuk-error-message');
  await expect(endDateError).toContainText('Enter an end date', {
    timeout: 15000,
  });

  await expect(page.locator('.nhsuk-u-visually-hidden').first()).toHaveText(
    'Error: ',
  );

  await expect(
    page.getByRole('link', { name: 'Back', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Back', exact: true }).click();

  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
});

test('Select dates to cancel error, must be in the future', async ({
  page,
}) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );

  await page.getByRole('button', { name: 'Continue to cancel' }).click();

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
  await page.getByRole('button', { name: 'Continue', exact: true }).click();

  // Assert both errors appear
  // We check for the specific error text your app generates for past dates
  const errorMessages = page.locator('.nhsuk-error-message');

  await expect(errorMessages.filter({ hasText: /Start date/ })).toContainText(
    /must be in the future/i,
    { timeout: 15000 },
  );

  await expect(errorMessages.filter({ hasText: /End date/ })).toContainText(
    /must be in the future/i,
    { timeout: 15000 },
  );

  await expect(
    page.getByRole('link', { name: 'Back', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Back', exact: true }).click();

  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
});

test('Select dates to cancel error, end date must be after the start date', async ({
  page,
}) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );

  await page.getByRole('button', { name: 'Continue to cancel' }).click();

  // Start is 5 days from now, End is 2 days from now
  const now = new Date();

  const startDate = new Date(now);
  startDate.setDate(now.getDate() + 5);

  const endDate = new Date(now);
  endDate.setDate(now.getDate() + 2);

  // Fill Start Date (The later date)
  await page.locator('#start-date-day').fill(startDate.getDate().toString());
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
  await page.locator('#end-date-year').fill(endDate.getFullYear().toString());

  // Submit
  await page.getByRole('button', { name: 'Continue', exact: true }).click();

  // Look for the specific sequence error
  const errorContainer = page
    .locator('.nhsuk-form-group--error')
    .filter({ hasText: 'End date' });
  await expect(errorContainer.locator('.nhsuk-error-message')).toContainText(
    /End date must be on or after the start date/i,
    { timeout: 15000 },
  );

  await expect(
    page.getByRole('link', { name: 'Back', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Back', exact: true }).click();

  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
});

test('Select dates to cancel error within 3 months - 91 days', async ({
  page,
}) => {
  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );

  await page.getByRole('button', { name: 'Continue to cancel' }).click();

  const now = new Date();

  const startDate = new Date(now);
  startDate.setDate(now.getDate() + 1);

  const endDate = new Date(now);
  endDate.setDate(startDate.getDate() + 90);

  // Fill Start Date
  await page.locator('#start-date-day').fill(startDate.getDate().toString());
  await page
    .locator('#start-date-month')
    .fill((startDate.getMonth() + 1).toString());
  await page
    .locator('#start-date-year')
    .fill(startDate.getFullYear().toString());

  // Fill End Date
  await page.locator('#end-date-day').fill(endDate.getDate().toString());
  await page
    .locator('#end-date-month')
    .fill((endDate.getMonth() + 1).toString());
  await page.locator('#end-date-year').fill(endDate.getFullYear().toString());

  // Trigger the validation
  await page.getByRole('button', { name: 'Continue', exact: true }).click();

  // Target the Start Date group by its specific legend
  const startDateGroup = page.locator('.nhsuk-form-group').filter({
    has: page.locator('legend').getByText('Start date', { exact: true }),
  });

  await expect(startDateGroup.locator('.nhsuk-error-message')).toContainText(
    'Start date must be',
    { timeout: 15000 },
  );

  // Target the End Date group by its specific legend
  const endDateGroup = page.locator('.nhsuk-form-group').filter({
    has: page.locator('legend').getByText('End date', { exact: true }),
  });

  await expect(endDateGroup.locator('.nhsuk-error-message')).toContainText(
    'End date must be',
    { timeout: 15000 },
  );

  await expect(
    page.getByRole('link', { name: 'Back', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Back', exact: true }).click();

  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
});
