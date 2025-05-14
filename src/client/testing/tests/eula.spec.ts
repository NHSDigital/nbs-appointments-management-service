import { test, expect } from '../fixtures';
import { EulaConsentPage, LoginPage } from '@testing-page-objects';

test.describe.configure({ mode: 'serial' });

test('A user with an out of date EULA consent version is prompted with the EULA consent page', async ({
  page,
  getTestUser,
}) => {
  await new LoginPage(page)
    .goto()
    .then(loginPage => loginPage.logInWithNhsMail())
    .then(async oAuthPage => {
      oAuthPage.signInExpectingEulaRedirect(getTestUser(5));

      return new EulaConsentPage(oAuthPage.page);
    })
    .then(async eulaConsentPage => {
      await expect(eulaConsentPage.title).toBeVisible();

      // Try to bypass EULA consent
      await page.goto('/manage-your-appointments/sites');

      await page.waitForURL('**/eula');
      await expect(eulaConsentPage.title).toBeVisible();
      await expect(
        page.getByRole('heading', {
          name: 'Manage your appointments',
        }),
      ).not.toBeVisible();
    });
});

test('A user with an out of date EULA version is prompted with the EULA consent page on login, but not again after they have consented', async ({
  page,
  getTestUser,
}) => {
  await new LoginPage(page)
    .goto()
    .then(loginPage => loginPage.logInWithNhsMail())
    .then(async oAuthPage => {
      oAuthPage.signInExpectingEulaRedirect(getTestUser(5));

      await page.waitForURL('**/eula');
      return new EulaConsentPage(oAuthPage.page);
    })
    .then(async eulaConsentPage => {
      await expect(eulaConsentPage.title).toBeVisible();

      // Try to bypass EULA consent
      await page.goto('/manage-your-appointments/sites');

      await page.waitForURL('**/eula');

      return await eulaConsentPage.acceptEula();
    })
    .then(async siteSelectionPage => {
      await expect(siteSelectionPage.title).toBeVisible();

      return siteSelectionPage.logOut();
    })
    .then(loginPage => loginPage.logInWithNhsMail())
    .then(async oAuthPage => oAuthPage.signIn(getTestUser(5)))
    .then(async siteSelectionPage => {
      await expect(siteSelectionPage.title).toBeVisible();
    });
});
