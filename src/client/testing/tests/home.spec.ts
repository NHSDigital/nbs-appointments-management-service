import { test, expect } from '../fixtures';
import { OAuthLoginPage, RootPage } from '@testing-page-objects';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;

let site1: Site;
let site2: Site;

test.beforeEach(async ({ page, getTestSite }) => {
  site1 = getTestSite(1);
  site2 = getTestSite(2);
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
    page.getByRole('cell').filter({ hasText: site2.name }),
  ).not.toBeVisible();
  await expect(
    page.getByRole('cell').filter({ hasText: site1.name }),
  ).toBeVisible();
});

test('An admin user loads home page, all sites are loaded', async ({
  page,
  getTestUser,
}) => {
  await oAuthPage.signIn(getTestUser(7));
  await expect(
    page.getByRole('cell').filter({ hasText: site2.name }),
  ).toBeVisible();
  await expect(
    page.getByRole('cell').filter({ hasText: site1.name }),
  ).toBeVisible();
});

test('A user loads home page and searches for a site, site list is filtered', async ({
  page,
  getTestUser,
}) => {
  await oAuthPage.signIn(getTestUser(6));
  await expect(
    page.getByRole('cell', { name: 'Church Lane Pharmacy' }),
  ).not.toBeVisible();
  await expect(page.getByText(/Robin Lane Medical Centre/)).toBeVisible();

  const searchInput = page.getByRole('textbox', {
    name: 'Search active sites by name or ODS code',
  });
  await searchInput.fill('Church');

  const searchButton = page.getByRole('button', { name: 'Search' });
  await searchButton.click();

  await expect(
    page.getByRole('cell', { name: 'Robin Lane Medical Centre' }),
  ).not.toBeVisible();
});
