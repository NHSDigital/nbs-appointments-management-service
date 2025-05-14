import { test, expect } from '../fixtures';
import { LoginPage } from '@testing-page-objects';
import { Site } from '@types';

let site1: Site;
let site2: Site;

test.beforeEach(async ({ getTestSite }) => {
  site1 = getTestSite(1);
  site2 = getTestSite(2);
});

test('A user loads home page, only sites with same scope are loaded', async ({
  page,
  getTestUser,
}) => {
  await new LoginPage(page)
    .goto()
    .then(loginPage => loginPage.logInWithNhsMail())
    .then(oAuthPage => oAuthPage.signIn(getTestUser(6)))
    .then(async () => {
      await expect(
        page.getByRole('link', { name: site2.name }),
      ).not.toBeVisible();
      await expect(page.getByRole('link', { name: site1.name })).toBeVisible();
    });
});

test('An admin user loads home page, all sites are loaded', async ({
  page,
  getTestUser,
}) => {
  await new LoginPage(page)
    .goto()
    .then(loginPage => loginPage.logInWithNhsMail())
    .then(oAuthPage => oAuthPage.signIn(getTestUser(7)))
    .then(async () => {
      await expect(page.getByRole('link', { name: site2.name })).toBeVisible();
      await expect(page.getByRole('link', { name: site1.name })).toBeVisible();
    });
});

test('A user loads home page and searches for a site, site list is filtered', async ({
  page,
  getTestUser,
}) => {
  await new LoginPage(page)
    .goto()
    .then(loginPage => loginPage.logInWithNhsMail())
    .then(oAuthPage => oAuthPage.signIn(getTestUser(6)))
    .then(async () => {
      await expect(
        page.getByRole('link', { name: site2.name }),
      ).not.toBeVisible();
      await expect(page.getByRole('link', { name: site1.name })).toBeVisible();

      const searchInput = page.getByRole('textbox', {
        name: 'site-search',
      });
      await searchInput.fill('Church');
      await expect(
        page.getByRole('link', { name: 'Robin Lane Medical Centre' }),
      ).not.toBeVisible();
    });
});
