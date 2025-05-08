import { test, expect } from '../../fixtures';
import { LoginPage } from '@testing-page-objects';

let rootPage: LoginPage;

test.beforeEach(async ({ page }) => {
  rootPage = new LoginPage(page);
  await page.context().clearCookies();
});

test('The user accepts analytics cookies', async ({ page }) => {
  await rootPage.goto();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).toBeVisible();

  await rootPage.cookieBanner.acceptCookiesButton.click();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
  await expect(rootPage.cookieBanner.postAcceptanceMessage).toBeVisible();

  await expect(
    (await page.context().cookies()).find(
      cookie => cookie.name === 'nhsuk-mya-cookie-consent',
    )?.value,
  ).toBe('%257B%2522consented%2522%253Atrue%252C%2522version%2522%253A1%257D');
});

test('The user rejects analytics cookies', async ({ page }) => {
  await rootPage.goto();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).toBeVisible();

  await rootPage.cookieBanner.rejectCookiesButton.click();

  await expect(rootPage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
  await expect(rootPage.cookieBanner.postAcceptanceMessage).toBeVisible();

  await expect(
    (await page.context().cookies()).find(
      cookie => cookie.name === 'nhsuk-mya-cookie-consent',
    )?.value,
  ).toBe('%257B%2522consented%2522%253Afalse%252C%2522version%2522%253A1%257D');
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
