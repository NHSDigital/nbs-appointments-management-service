import { test, expect } from '@playwright/test';
import OAuthLoginPage from './page-objects/oauth';
import RootPage from './page-objects/root';
import SiteSelectionPage from './page-objects/site-selection';

test('User visits the site origin, signs in and see the Site Selection menu', async ({
  page,
}) => {
  const rootPage = new RootPage(page);
  const oAuthPage = new OAuthLoginPage(page);
  const siteSelectionPage = new SiteSelectionPage(page);

  await rootPage.goto();
  await rootPage.logInButton.click();

  await oAuthPage.signIn();

  await expect(rootPage.logOutButton).toBeVisible();
  await expect(siteSelectionPage.siteSelectionCardHeading).toBeVisible();
});

test('User visits the site origin, signs in, then signs out again', async ({
  page,
}) => {
  const rootPage = new RootPage(page);
  const oAuthPage = new OAuthLoginPage(page);
  const siteSelectionPage = new SiteSelectionPage(page);

  await rootPage.goto();
  await rootPage.logInButton.click();
  await oAuthPage.signIn();

  await expect(rootPage.logOutButton).toBeVisible();
  await expect(siteSelectionPage.siteSelectionCardHeading).toBeVisible();

  await expect(siteSelectionPage.logOutButton).toBeVisible();
  await siteSelectionPage.logOutButton.click();

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
