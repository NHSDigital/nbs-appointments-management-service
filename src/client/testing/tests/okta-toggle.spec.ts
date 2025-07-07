import { skip } from 'node:test';
import {
  test,
  expect,
  overrideFeatureFlag,
  clearAllFeatureFlagOverrides,
} from '../fixtures';
import {
  ManageUserPage,
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
  UsersPage,
} from '@testing-page-objects';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

test.describe.configure({ mode: 'serial' });

[true, false].forEach(oktaEnabled => {
  test.describe(`Tests for OKTA enabled: '${oktaEnabled}'`, () => {
    test.beforeAll(async () => {
      await overrideFeatureFlag('OktaEnabled', oktaEnabled);
    });

    test.afterAll(async () => {
      await clearAllFeatureFlagOverrides();
    });

    test.describe('Login tests', () => {
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

    test.describe('Create user tests', () => {
      let rootPage: RootPage;
      let oAuthPage: OAuthLoginPage;
      let siteSelectionPage: SiteSelectionPage;
      let sitePage: SitePage;
      let usersPage: UsersPage;
      let manageUserPage: ManageUserPage;

      test.beforeEach(async ({ page, getTestSite }) => {
        const site = getTestSite();
        rootPage = new RootPage(page);
        oAuthPage = new OAuthLoginPage(page);
        siteSelectionPage = new SiteSelectionPage(page);
        sitePage = new SitePage(page);
        usersPage = new UsersPage(page);
        manageUserPage = new ManageUserPage(page);

        await rootPage.goto();
        await rootPage.pageContentLogInButton.click();
        await oAuthPage.signIn();
        await siteSelectionPage.selectSite(site);
        await sitePage.userManagementCard.click();
        await page.waitForURL(`**/site/${site.id}/users`);
      });

      test('The current user creates a new NHSMail user with some roles', async ({
        page,
        getTestSite,
        newUserName,
      }) => {
        const newUser = newUserName(test.info());
        await usersPage.addUserButton.click();
        await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

        await expect(manageUserPage.emailStep.title).toBeVisible();

        if (oktaEnabled) {
          await expect(manageUserPage.emailStep.emailHint).toBeVisible();
        } else {
          await expect(manageUserPage.emailStep.emailHint).not.toBeVisible();
        }

        await manageUserPage.emailStep.emailInput.fill(newUser);
        await manageUserPage.emailStep.continueButton.click();

        await expect(manageUserPage.rolesStep.title).toBeVisible();
        await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
        await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
        await manageUserPage.rolesStep.continueButton.click();

        await expect(manageUserPage.summaryStep.title).toBeVisible();
        await manageUserPage.summaryStep.continueButton.click();

        await usersPage.userExists(newUser);
        await usersPage.verifyUserRoles('Appointment manager', newUser);
        await usersPage.verifyUserRoles('Availability manager', newUser);
      });

      test('The current user creates a new Okta user with some roles', async ({
        page,
        getTestSite,
        externalUserName,
      }) => {
        const externalUser = externalUserName(test.info());
        // TODO: Remove when Okta feature flag is no longer needed
        if (!oktaEnabled) {
          skip();
          return;
        }

        await usersPage.addUserButton.click();
        await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

        await expect(manageUserPage.emailStep.title).toBeVisible();

        if (oktaEnabled) {
          await expect(manageUserPage.emailStep.emailHint).toBeVisible();
        } else {
          await expect(manageUserPage.emailStep.emailHint).not.toBeVisible();
        }

        await manageUserPage.emailStep.emailInput.fill(externalUser);
        await manageUserPage.emailStep.continueButton.click();

        await expect(manageUserPage.namesStep.title).toBeVisible();
        await manageUserPage.namesStep.firstNameInput.fill('Elizabeth');
        await manageUserPage.namesStep.lastNameInput.fill('Kensington-Jones');
        await manageUserPage.namesStep.continueButton.click();

        await expect(manageUserPage.rolesStep.title).toBeVisible();
        await manageUserPage.rolesStep.appointmentManagerCheckbox.check();
        await manageUserPage.rolesStep.availabilityManagerCheckbox.check();
        await manageUserPage.rolesStep.continueButton.click();

        await expect(manageUserPage.summaryStep.title).toBeVisible();
        await expect(manageUserPage.summaryStep.nameSummary).toHaveText(
          'Elizabeth Kensington-Jones',
        );
        await expect(manageUserPage.summaryStep.rolesSummary).toHaveText(
          'Appointment manager, Availability manager',
        );
        await expect(manageUserPage.summaryStep.emailAddressSummary).toHaveText(
          externalUser,
        );
        await manageUserPage.summaryStep.continueButton.click();

        await usersPage.userExists(externalUser);
        await usersPage.verifyUserRoles('Appointment manager', externalUser);
        await usersPage.verifyUserRoles('Availability manager', externalUser);
      });

      test('The current user tries to create a new user without any roles', async ({
        page,
        getTestSite,
        newUserName,
      }) => {
        await usersPage.addUserButton.click();
        await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

        await expect(manageUserPage.emailStep.title).toBeVisible();

        if (oktaEnabled) {
          await expect(manageUserPage.emailStep.emailHint).toBeVisible();
        } else {
          await expect(manageUserPage.emailStep.emailHint).not.toBeVisible();
        }

        await manageUserPage.emailStep.emailInput.fill(
          newUserName(test.info()),
        );
        await manageUserPage.emailStep.continueButton.click();

        await expect(manageUserPage.rolesStep.title).toBeVisible();
        await manageUserPage.rolesStep.continueButton.click();

        await expect(
          page.getByText('You have not selected any roles for this user'),
        ).toBeVisible();
      });

      test('The current user creates a new user but enters the email of an existing user, and are able to edit this user', async ({
        page,
        getTestSite,
      }) => {
        await usersPage.addUserButton.click();
        await page.waitForURL(`**/site/${getTestSite().id}/users/manage`);

        await expect(manageUserPage.emailStep.title).toBeVisible();

        if (oktaEnabled) {
          await expect(manageUserPage.emailStep.emailHint).toBeVisible();
        } else {
          await expect(manageUserPage.emailStep.emailHint).not.toBeVisible();
        }

        await manageUserPage.emailStep.emailInput.fill(
          'zzz_test_user_3@nhs.net',
        );
        await manageUserPage.emailStep.continueButton.click();

        await expect(manageUserPage.rolesStep.title).toBeVisible();
        await expect(
          manageUserPage.rolesStep.availabilityManagerCheckbox,
        ).toBeChecked();
      });
    });
  });
});
