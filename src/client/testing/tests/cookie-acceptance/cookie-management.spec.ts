import { test, expect } from '../../fixtures';
import CookiesPolicyPage from '../../page-objects/cookies-policy';
import RootPage from '../../page-objects/root';

let rootPage: RootPage;
let cookiesPolicyPage: CookiesPolicyPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
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

  const consentCookie = (await page.context().cookies()).find(
    cookie => cookie.name === 'nhsuk-mya-cookie-consent',
  );

  expect(consentCookie?.value).toBe(
    '%257B%2522consented%2522%253Atrue%252C%2522version%2522%253A1%257D',
  );
});
