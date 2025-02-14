import { test, expect } from '../../fixtures';
import RootPage from '../../page-objects/root';

let rootPage: RootPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
});

test('The user accepts analytics cookies', async ({ page }) => {
  await rootPage.goto();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).toBeVisible();

  await rootPage.cookieBanner.acceptCookiesButton.click();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
  await expect(rootPage.cookieBanner.postAcceptanceMessage).toBeVisible();

  const consentCookie = (await page.context().cookies()).find(
    cookie => cookie.name === 'nhsuk-mya-cookie-consent',
  );

  expect(consentCookie?.value).toBe(
    '%7B%22consented%22%3Atrue%2C%22version%22%3A5%7D',
  );
});

test('The user rejects analytics cookies', async ({ page }) => {
  await rootPage.goto();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).toBeVisible();

  await rootPage.cookieBanner.rejectCookiesButton.click();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
  await expect(rootPage.cookieBanner.postAcceptanceMessage).toBeVisible();

  const consentCookie = (await page.context().cookies()).find(
    cookie => cookie.name === 'nhsuk-mya-cookie-consent',
  );

  expect(consentCookie?.value).toBe(
    '%7B%22consented%22%3Afalse%2C%22version%22%3A5%7D',
  );
});

test('The banner only shows when preferences are not set', async ({ page }) => {
  await rootPage.goto();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).toBeVisible();

  await rootPage.cookieBanner.acceptCookiesButton.click();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
  await expect(rootPage.cookieBanner.postAcceptanceMessage).toBeVisible();

  await page.reload();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
  await expect(rootPage.cookieBanner.postAcceptanceMessage).toBeVisible();

  await page.context().clearCookies();
  await page.reload();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).toBeVisible();
});
