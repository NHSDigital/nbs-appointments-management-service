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
});

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();

  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );

  // await expect(
  //   page.getByRole('heading', { name: 'Before you continue' }),
  // ).toBeVisible();

  await page
    .getByRole('button', { name: 'Continue to cancel' })
    .click({ delay: 100 });
});

test('Cannot cancel these sessions - Return to view availability', async ({
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

  await page
    .getByRole('button', { name: 'Continue', exact: true })
    .click({ delay: 100 });

  await page
    .getByRole('button', { name: 'Return to view availability' })
    .click({ delay: 100 });

  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/view-availability`,
    { timeout: 15000 },
  );
});

test('Cannot cancel these sessions - Select different dates', async ({
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

  await page
    .getByRole('button', { name: 'Continue', exact: true })
    .click({ delay: 100 });

  await expect(
    page.getByRole('button', { name: 'Select different dates', exact: true }),
  ).toBeVisible({ timeout: 15000 });

  await page
    .getByRole('button', { name: 'Select different dates' })
    .click({ delay: 100 });

  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
    { timeout: 15000 },
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
