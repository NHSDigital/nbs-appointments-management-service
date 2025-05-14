import {
  test,
  expect,
  overrideFeatureFlag,
  clearAllFeatureFlagOverrides,
} from '../fixtures';
import { LoginPage } from '@testing-page-objects';
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
      await new LoginPage(page)
        .goto()
        .then(async loginPage => {
          if (oktaEnabled) {
            await expect(loginPage.OKTALogInLink).toBeVisible();
          } else {
            await expect(loginPage.OKTALogInLink).not.toBeVisible();
          }

          return loginPage.logInWithNhsMail();
        })
        .then(oAuthPage => oAuthPage.signIn())
        .then(async siteSelectionPage => {
          await expect(siteSelectionPage.logOutButton).toBeVisible();
          await expect(
            siteSelectionPage.siteSelectionCardHeading,
          ).toBeVisible();
        });
    });

    test('User visits the site origin, signs in, then signs out again', async ({
      page,
    }) => {
      await new LoginPage(page)
        .goto()
        .then(async loginPage => {
          return loginPage.logInWithNhsMail();
        })
        .then(oAuthPage => oAuthPage.signIn())
        .then(async siteSelectionPage => {
          await expect(siteSelectionPage.logOutButton).toBeVisible();
          await expect(
            siteSelectionPage.siteSelectionCardHeading,
          ).toBeVisible();

          await expect(siteSelectionPage.logOutButton).toBeVisible();
          return siteSelectionPage.logOut();
        })
        .then(async logInPage => {
          await expect(
            logInPage.page.getByRole('heading', {
              name: 'Manage your appointments',
            }),
          ).toBeVisible();

          await expect(
            logInPage.page.getByText(
              'You are currently not signed in. You must sign in to access this service.',
            ),
          ).toBeVisible();

          await expect(logInPage.nhsMailLogInButton).toBeVisible();
        });
    });

    test('Users with no roles at any site but valid auth credentials can still sign in', async ({
      page,
      getTestUser,
    }) => {
      await new LoginPage(page)
        .goto()
        .then(async loginPage => {
          return loginPage.logInWithNhsMail();
        })
        .then(oAuthPage => oAuthPage.signIn(getTestUser(4)))
        .then(async siteSelectionPage => {
          await expect(siteSelectionPage.noSitesMessage).toBeVisible();
        });
    });
  });
});
