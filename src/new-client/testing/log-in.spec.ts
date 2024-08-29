import { test, expect } from '@playwright/test';
import OAuthLoginPage from './page-objects/oauth';
import RootPage from './page-objects/root';
import SiteSelection from './page-objects/site-selection';

test('User visits the site origin, signs in and see the Site Selection menu', async ({
  page,
}) => {
  const rootPage = new RootPage(page);
  const oAuthPage = new OAuthLoginPage(page);
  const siteSelection = new SiteSelection(page);

  await rootPage.goto();
  await rootPage.logInButton.click();

  await oAuthPage.signIn();

  await expect(rootPage.logOutButton).toBeVisible();
  await expect(siteSelection.siteSelectionCardHeading).toBeVisible();
});

test('User visits the site origin, signs in, then signs out again', async ({
  page,
}) => {
  const rootPage = new RootPage(page);
  const oAuthPage = new OAuthLoginPage(page);
  const siteSelection = new SiteSelection(page);

  await rootPage.goto();
  await rootPage.logInButton.click();
  await oAuthPage.signIn();

  await expect(rootPage.logOutButton).toBeVisible();
  await expect(siteSelection.siteSelectionCardHeading).toBeVisible();

  await expect(siteSelection.logOutButton).toBeVisible();
  await siteSelection.logOutButton.click();

  await expect(
    page.getByRole('heading', { name: 'You cannot access this site' }),
  ).toBeVisible();

  await expect(
    page.getByText(
      'You are currently not signed in. To use this site, please sign in.',
    ),
  ).toBeVisible();

  await expect(rootPage.logInButton).toBeVisible();
});
