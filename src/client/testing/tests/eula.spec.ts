import { test, expect } from '../fixtures';
import {
  EulaConsentPage,
  OAuthLoginPage,
  RootPage,
  SiteSelectionPage,
} from '@testing-page-objects';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let eulaConsentPage: EulaConsentPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  eulaConsentPage = new EulaConsentPage(page);
});

test.describe.configure({ mode: 'serial' });

test('A user with an out of date EULA consent version is prompted with the EULA consent page', async ({
  page,
  getTestUser,
}) => {
  const user5 = getTestUser(5);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();

  await oAuthPage.page.getByLabel('Username').fill(user5.username);
  await oAuthPage.page.getByLabel('Password').fill(user5.password);
  await oAuthPage.page.getByLabel('Password').press('Enter');

  await page.waitForURL('**/eula');
  await expect(eulaConsentPage.title).toBeVisible();

  // Try to bypass EULA consent
  await page.goto('/manage-your-appointments/sites');

  await page.waitForURL('**/eula');
  await expect(eulaConsentPage.title).toBeVisible();
  await expect(siteSelectionPage.title).not.toBeVisible();
});

test('A user with an out of date EULA version is prompted with the EULA consent page on login, but not again after they have consented', async ({
  page,
  getTestUser,
}) => {
  const user5 = getTestUser(5);
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();

  await oAuthPage.page.getByLabel('Username').fill(user5.username);
  await oAuthPage.page.getByLabel('Password').fill(user5.password);
  await oAuthPage.page.getByLabel('Password').press('Enter');

  await page.waitForURL('**/eula');
  await expect(eulaConsentPage.title).toBeVisible();

  await eulaConsentPage.acceptAndContinueButton.click();

  await page.waitForURL('**/');
  await expect(siteSelectionPage.title).toBeVisible();

  await rootPage.logOutButton.click();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.page.getByLabel('Username').fill(user5.username);
  await oAuthPage.page.getByLabel('Password').fill(user5.password);
  await oAuthPage.page.getByLabel('Password').press('Enter');

  // do not expect to see the EULA consent page again
  await page.waitForURL('**/');
  await expect(siteSelectionPage.title).toBeVisible();
});
