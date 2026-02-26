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
  site = getTestSite(1);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();

  await page.goto('/manage-your-appointments/sites');
  await page.waitForURL(`/manage-your-appointments/sites`);
  await page.getByRole('link', { name: 'View Church Lane Pharmacy' }).click();
  await page
    .getByRole('link', { name: 'View availability and manage' })
    .click();
});

test('Cancel a date range monthly page', async ({ page }) => {
  await expect(
    page.getByRole('button', { name: 'Change availability' }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Change availability' }).click();
  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
  await expect(
    page.getByRole('link', { name: 'Back', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Back', exact: true }).click();
  await page.goto(
    `/manage-your-appointments/site/${site.id}/view-availability`,
  );
  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/view-availability`,
  );
});

test('Cancel a date range weekly page', async ({ page }) => {
  await page
    .getByRole('listitem')
    .filter({ hasText: '23 February to 1 March' })
    .getByRole('link')
    .click();
  await expect(
    page.getByRole('button', { name: 'Change availability' }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Change availability' }).click();
  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
  await expect(page).toHaveURL(/.*\/change-availability/);
  await expect(
    page.getByRole('link', { name: 'Back', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Back', exact: true }).click();
  await page.goto(
    `/manage-your-appointments/site/${site.id}/view-availability/week?date=.*`,
  );
  // Verify weekly page
  await expect(page).toHaveURL(/.*\/view-availability(\/week)?/);
});

test('Cancel a date range daily page', async ({ page }) => {
  await page
    .getByRole('listitem')
    .filter({ hasText: '23 February to 1 March' })
    .getByRole('link')
    .click();

  await expect(page).toHaveURL(/.*\/view-availability\/week/);
  const wednesdaySection = page
    .locator('li')
    .filter({ hasText: 'Wednesday 25 February' });
  await expect(
    wednesdaySection.getByText(/Total appointments: [1-9]\d*/),
  ).toBeVisible({ timeout: 20000 });
  // If the Total appointments: 0 then the "View daily appointments" link will not be visible
  await page.getByRole('link', { name: 'View daily appointments' }).click();
  await expect(
    page.getByRole('button', { name: 'Change availability' }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Change availability' }).click();
  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
  await expect(page).toHaveURL(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );
  await expect(
    page.getByRole('link', { name: 'Back', exact: true }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'Back', exact: true }).click();
  await page.goto(
    `/manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=2026-02-25&page=1`,
  );
  // Verify the daily page
  await expect(page).toHaveURL(/.*\/view-availability(\/daily-appointments)?/);
});
