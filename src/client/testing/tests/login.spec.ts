import { test, expect } from '../fixtures';
import {
  OAuthLoginPage,
  RootPage,
  SiteSelectionPage,
} from '@testing-page-objects';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

test.describe.configure({ mode: 'serial' });

test.describe('Login tests', () => {
  test('User visits the site origin, signs in and see the Site Selection menu', async ({
    page,
  }) => {
    const rootPage = new RootPage(page);
    const oAuthPage = new OAuthLoginPage(page);
    const siteSelectionPage = new SiteSelectionPage(page);

    expect(rootPage.OKTALogInButton).toBeVisible();

    await rootPage.goto();
    await rootPage.pageContentLogInButton.click();

    await oAuthPage.signIn();

    await expect(rootPage.logOutButton).toBeVisible();
    await expect(siteSelectionPage.title).toBeVisible();
  });

  test('User visits the site origin, signs in, then signs out again', async ({
    page,
  }) => {
    const rootPage = new RootPage(page);
    const oAuthPage = new OAuthLoginPage(page);
    const siteSelectionPage = new SiteSelectionPage(page);

    await rootPage.goto();
    await rootPage.pageContentLogInButton.click();
    await oAuthPage.signIn();

    await expect(rootPage.logOutButton).toBeVisible();
    await expect(siteSelectionPage.title).toBeVisible();

    await expect(siteSelectionPage.logOutButton).toBeVisible();
    await siteSelectionPage.logOutButton.click();

    await page.waitForURL('**/login');

    await expect(
      page.getByRole('heading', { name: 'Manage your appointments' }),
    ).toBeVisible();

    await expect(
      page.getByText(
        'You are currently not signed in. You must sign in to access this service.',
      ),
    ).toBeVisible();

    await expect(rootPage.pageContentLogInButton).toBeVisible();
  });

  test('Users with no roles at any site but valid auth credentials can still sign in', async ({
    page,
    getTestUser,
  }) => {
    const rootPage = new RootPage(page);
    const oAuthPage = new OAuthLoginPage(page);
    const siteSelectionPage = new SiteSelectionPage(page);

    await rootPage.goto();
    await rootPage.pageContentLogInButton.click();
    await oAuthPage.signIn(getTestUser(4));

    await expect(siteSelectionPage.noSitesMessage).toBeVisible();
  });
});
