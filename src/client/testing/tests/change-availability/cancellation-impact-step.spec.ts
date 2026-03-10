import { OAuthLoginPage, RootPage } from '@testing-page-objects';
import { Site } from '@types';
import { test, expect, overrideFeatureFlag } from '../../fixtures';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;

let site: Site;

test.describe.configure({ mode: 'serial' });

test.beforeAll(async () => {
  await overrideFeatureFlag('CancelADateRange', true);
  await overrideFeatureFlag('CancelADateRangeWithBookings', true);
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

  await page.waitForURL(`/manage-your-appointments/sites`);
  await page.getByRole('link', { name: 'View Church Lane Pharmacy' }).click();
  await page.waitForURL(`/manage-your-appointments/site/${site.id}`);
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
});

test('There are no sessions in this date range - Choose a new date range', async ({
  page,
}) => {
  const now = new Date();

  const startDate = new Date(now);
  startDate.setDate(now.getDate() + 1);

  const endDate = new Date(now);
  endDate.setDate(now.getDate() + 23);

  await page.locator('#start-date-day').fill(startDate.getDate().toString());
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
  await page.locator('#end-date-year').fill(endDate.getFullYear().toString());
  await page.getByRole('button', { name: 'Continue', exact: true }).click();
  await page.waitForURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
  await page.getByRole('button', { name: 'Choose a new date range' }).click();

  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
    { timeout: 15000 },
  );

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
