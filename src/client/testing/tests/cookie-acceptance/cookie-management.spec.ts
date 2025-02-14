import { test, expect } from '../../fixtures';
import CookiesPolicyPage from '../../page-objects/cookies-policy';
import RootPage from '../../page-objects/root';

let rootPage: RootPage;
let cookiesPolicyPage: CookiesPolicyPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  cookiesPolicyPage = new CookiesPolicyPage(page);
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
    '%7B%22consented%22%3Atrue%2C%22version%22%3A5%7D',
  );
});
