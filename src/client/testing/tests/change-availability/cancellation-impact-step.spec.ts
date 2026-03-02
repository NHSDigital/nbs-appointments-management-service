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

  await page.goto('/manage-your-appointments/sites');
  await page.waitForURL(`/manage-your-appointments/sites`);
  await page.getByRole('link', { name: 'View Church Lane Pharmacy' }).click();
  await page
    .getByRole('link', { name: 'View availability and manage' })
    .click();
});

test('There are no sessions in this date range - Choose a new date range', async ({
  page,
}) => {
  await page.getByRole('button', { name: 'Change availability' }).click();
  await page.getByRole('button', { name: 'Continue to cancel' }).click();
  await page
    .getByRole('group', { name: 'Start date' })
    .getByLabel('Day')
    .click();
  await page
    .getByRole('group', { name: 'Start date' })
    .getByLabel('Day')
    .fill('3');
  await page
    .getByRole('group', { name: 'Start date' })
    .getByLabel('Month')
    .click();
  await page
    .getByRole('group', { name: 'Start date' })
    .getByLabel('Month')
    .fill('3');
  await page
    .getByRole('group', { name: 'Start date' })
    .getByLabel('Year')
    .click();
  await page
    .getByRole('group', { name: 'Start date' })
    .getByLabel('Year')
    .fill('2026');
  await page.getByRole('group', { name: 'End date' }).getByLabel('Day').click();
  await page
    .getByRole('group', { name: 'End date' })
    .getByLabel('Day')
    .fill('25');
  await page
    .getByRole('group', { name: 'End date' })
    .getByLabel('Month')
    .click();
  await page
    .getByRole('group', { name: 'End date' })
    .getByLabel('Month')
    .fill('3');
  await page
    .getByRole('group', { name: 'End date' })
    .getByLabel('Year')
    .click();
  await page
    .getByRole('group', { name: 'End date' })
    .getByLabel('Year')
    .fill('2026');
  await page.getByRole('button', { name: 'Continue' }).click();
  await page.getByRole('button', { name: 'Choose a new date range.' }).click();
});
