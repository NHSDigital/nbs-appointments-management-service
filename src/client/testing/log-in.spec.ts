import {
  test,
  expect,
  overrideFeatureFlag,
  clearAllFeatureFlagOverrides,
} from './fixtures';
import OAuthLoginPage from './page-objects/oauth';
import RootPage from './page-objects/root';
import SiteSelectionPage from './page-objects/site-selection';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

test.describe.configure({ mode: 'serial' });

[true, false].forEach(oktaEnabled => {
  test.describe(`Login Tests for OKTA enabled: '${oktaEnabled}'`, () => {
    test.beforeAll(async () => {
      await overrideFeatureFlag('OktaEnabled', oktaEnabled);
    });

    test.afterAll(async () => {
      await clearAllFeatureFlagOverrides();
    });

    test('User visits the site origin, signs in and see the Site Selection menu', async ({
      page,
    }) => {
      const rootPage = new RootPage(page);
      const oAuthPage = new OAuthLoginPage(page);
      const siteSelectionPage = new SiteSelectionPage(page);

      if (oktaEnabled) {
        expect(rootPage.OKTALogInButton).toBeVisible();
      } else {
        expect(rootPage.OKTALogInButton).not.toBeVisible();
      }

      await rootPage.goto();
      await rootPage.pageContentLogInButton.click();

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
      await rootPage.pageContentLogInButton.click();
      await oAuthPage.signIn();

      await expect(rootPage.logOutButton).toBeVisible();
      await expect(siteSelectionPage.siteSelectionCardHeading).toBeVisible();

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
});
