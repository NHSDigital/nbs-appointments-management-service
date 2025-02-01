import { test, expect } from './fixtures';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';

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
  getTestUser,
}) => {
  const user6 = getTestUser(6);
  await oAuthPage.signIn(user6);
  await expect(
    page.getByRole('link', { name: 'Church Lane Pharmacy' }),
  ).not.toBeVisible();
  await expect(
    page.getByRole('link', { name: 'Robin Lane Medical Centre' }),
  ).toBeVisible();
});

test('An admin user loads home page, all sites are loaded', async ({
  page,
  getTestUser,
}) => {
  await oAuthPage.signIn(getTestUser(7));
  await expect(
    page.getByRole('link', { name: 'Church Lane Pharmacy' }),
  ).toBeVisible();
  await expect(
    page.getByRole('link', { name: 'Robin Lane Medical Centre' }),
  ).toBeVisible();
});

test('A user loads home page and searches for a site, site list is filtered', async ({
  page,
}) => {
  await oAuthPage.signIn(env.TEST_USERS.testUser6);
  await expect(
    page.getByRole('link', { name: 'Church Lane Pharmacy' }),
  ).not.toBeVisible();
  await expect(
    page.getByRole('link', { name: 'Robin Lane Medical Centre' }),
  ).toBeVisible();

  const searchInput = page.getByRole('textbox', {
    name: 'site-search',
  });
  await searchInput.fill('Church');
  await expect(
    page.getByRole('link', { name: 'Robin Lane Medical Centre' }),
  ).not.toBeVisible();
});
