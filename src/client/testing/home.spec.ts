import { test, expect } from '@playwright/test';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import env from './testEnvironment';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
});

test('A user loads home page, only sites with same scope are loaded', async ({
  page,
}) => {
  await oAuthPage.signIn(env.TEST_USERS.testUser6);
  await expect(
    page.getByRole('link', { name: 'Church Lane Pharmacy' }),
  ).not.toBeVisible();
  await expect(
    page.getByRole('link', { name: 'Robin Lane Medical Centre' }),
  ).toBeVisible();
});

test('An admin user loads home page, all sites are loaded', async ({
  page,
}) => {
  await oAuthPage.signIn(env.TEST_USERS.adminTestUser);
  await expect(
    page.getByRole('link', { name: 'Church Lane Pharmacy' }),
  ).toBeVisible();
  await expect(
    page.getByRole('link', { name: 'Robin Lane Medical Centre' }),
  ).toBeVisible();
});
