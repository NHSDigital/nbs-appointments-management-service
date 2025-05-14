import { test, expect } from '../../fixtures';

test.beforeEach(async ({ page }) => {
  await page.context().clearCookies();
});

test('The user accepts analytics cookies', async ({ signInToSite }) => {
  await signInToSite().then(async sitePage => {
    await expect(sitePage.cookieBanner.preAcceptanceHeader).toBeVisible();

    await sitePage.cookieBanner.acceptCookiesButton.click();

    await expect(sitePage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
    await expect(sitePage.cookieBanner.postAcceptanceMessage).toBeVisible();

    await expect(
      (await sitePage.page.context().cookies()).find(
        cookie => cookie.name === 'nhsuk-mya-cookie-consent',
      )?.value,
    ).toBe(
      '%257B%2522consented%2522%253Atrue%252C%2522version%2522%253A1%257D',
    );
  });
});

test('The user rejects analytics cookies', async ({ signInToSite }) => {
  await signInToSite().then(async sitePage => {
    await expect(sitePage.cookieBanner.preAcceptanceHeader).toBeVisible();

    await sitePage.cookieBanner.rejectCookiesButton.click();

    await expect(sitePage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
    await expect(sitePage.cookieBanner.postAcceptanceMessage).toBeVisible();

    await expect(
      (await sitePage.page.context().cookies()).find(
        cookie => cookie.name === 'nhsuk-mya-cookie-consent',
      )?.value,
    ).toBe(
      '%257B%2522consented%2522%253Afalse%252C%2522version%2522%253A1%257D',
    );
  });
});

test('The banner only shows when preferences are not set', async ({
  page,
  signInToSite,
}) => {
  await signInToSite().then(async sitePage => {
    await expect(sitePage.cookieBanner.preAcceptanceHeader).toBeVisible();

    await sitePage.cookieBanner.acceptCookiesButton.click();

    await expect(sitePage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
    await expect(sitePage.cookieBanner.postAcceptanceMessage).toBeVisible();

    await page.reload();
    await expect(sitePage.cookieBanner.preAcceptanceHeader).not.toBeVisible();
    await expect(sitePage.cookieBanner.postAcceptanceMessage).toBeVisible();

    await page.context().clearCookies();
    await page.reload();

    await expect(sitePage.cookieBanner.preAcceptanceHeader).toBeVisible();
  });
});
