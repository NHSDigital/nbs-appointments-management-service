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

test('Verify 3 steps when bookings exist', async ({ page }) => {
  // With bookings
  await overrideFeatureFlag('CancelADateRangeWithBookings', true);

  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );

  const listItems = page
    .locator('ol li')
    .filter({ hasNot: page.getByRole('button') });

  // Confirming 3 items in the list
  await expect(listItems).toHaveText([
    'Cancel the sessions you want to change',
    'Choose to keep existing bookings',
    'Create new sessions with the updated details',
  ]);
});

test('Verify 2 steps when no bookings exist', async ({ page }) => {
  // Without bookings
  await overrideFeatureFlag('CancelADateRangeWithBookings', false);

  await page.goto(
    `/manage-your-appointments/site/${site.id}/change-availability`,
  );

  const listItems = page
    .locator('ol li')
    .filter({ hasNot: page.getByRole('button') });

  // Confirming 2 items in the list
  await expect(listItems).toHaveText([
    'Cancel the sessions you want to change',
    'Create new sessions with the updated details',
  ]);
});
