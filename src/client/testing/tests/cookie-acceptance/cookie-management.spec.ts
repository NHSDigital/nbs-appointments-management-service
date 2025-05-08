import { test, expect } from '../../fixtures';
import { CookiesPolicyPage, LoginPage } from '@testing-page-objects';

let rootPage: LoginPage;
let cookiesPolicyPage: CookiesPolicyPage;

test.beforeEach(async ({ page }) => {
  rootPage = new LoginPage(page);
  cookiesPolicyPage = new CookiesPolicyPage(page);
  await page.context().clearCookies();
});

test('The user can change their cookie acceptance', async ({ page }) => {
  await rootPage.goto();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).toBeVisible();

  await rootPage.cookieBanner.acceptCookiesButton.click();

  await rootPage.footerLinks.cookiesPolicy.click();
  await page.waitForURL('**/cookies-policy');

  await expect(cookiesPolicyPage.title).toBeVisible();

  await expect(
    cookiesPolicyPage.manageCookieAcceptanceForm.consentedRadio,
  ).toBeChecked();

  await cookiesPolicyPage.manageCookieAcceptanceForm.rejectedRadio.click();
  await cookiesPolicyPage.manageCookieAcceptanceForm.submitButton.click();

  await page.waitForURL('**/login?redirectUrl=/sites');
  await expect(rootPage.cookieBanner.postAcceptanceMessage).toBeVisible();

  await expect(
    (await page.context().cookies()).find(
      cookie => cookie.name === 'nhsuk-mya-cookie-consent',
    )?.value,
  ).toBe('%257B%2522consented%2522%253Afalse%252C%2522version%2522%253A1%257D');
});
