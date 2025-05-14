import { test, expect } from '../../fixtures';

test.beforeEach(async ({ page }) => {
  await page.context().clearCookies();
});

test('The user can change their cookie acceptance', async ({
  signInToSite,
  page,
}) => {
  await signInToSite()
    .then(async sitePage => {
      await expect(sitePage.cookieBanner.preAcceptanceHeader).toBeVisible();

      await sitePage.cookieBanner.acceptCookiesButton.click();

      return sitePage.clickCookiesFooterLink();
    })
    .then(async cookiesPolicyPage => {
      await expect(cookiesPolicyPage.title).toBeVisible();

      await expect(
        cookiesPolicyPage.manageCookieAcceptanceForm.consentedRadio,
      ).toBeChecked();

      await cookiesPolicyPage.manageCookieAcceptanceForm.rejectedRadio.click();
      return cookiesPolicyPage.saveCookiePreferences();
    })
    .then(async siteSelectionPage => {
      await expect(
        siteSelectionPage.cookieBanner.postAcceptanceMessage,
      ).toBeVisible();

      await expect(
        (await page.context().cookies()).find(
          cookie => cookie.name === 'nhsuk-mya-cookie-consent',
        )?.value,
      ).toBe(
        '%257B%2522consented%2522%253Afalse%252C%2522version%2522%253A1%257D',
      );
    });
});
