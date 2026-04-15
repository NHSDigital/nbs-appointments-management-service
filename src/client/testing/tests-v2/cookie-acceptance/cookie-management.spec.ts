import { test, expect } from '../../fixtures-v2';
import { CookiePoliciesPage } from '@e2etests/page-objects';

let cookiesPolicyPage: CookiePoliciesPage;

test.beforeEach(async ({ page }) => {
  cookiesPolicyPage = new CookiePoliciesPage(page);
  await page.context().clearCookies();
});

test('The user can change their cookie acceptance', async ({ page, setup }) => {
  await setup({}).then(async () => {
    // Initial acceptance via banner
    await expect(
      cookiesPolicyPage.cookieBanner.preAcceptanceHeader,
    ).toBeVisible();
    await cookiesPolicyPage.cookieBanner.acceptCookiesButton.click();

    await expect(
      cookiesPolicyPage.cookieBanner.preAcceptanceHeader,
    ).not.toBeVisible();
    await expect(
      cookiesPolicyPage.cookieBanner.postAcceptanceMessage,
    ).toBeVisible();

    // Navigate to Cookies Policy via Footer
    await cookiesPolicyPage.footer.links.cookiesPolicy.click();
    await page.waitForURL('**/cookies-policy');

    await expect(cookiesPolicyPage.title).toBeVisible();

    // Verify the "Accepted" state is reflected in the form
    await expect(
      cookiesPolicyPage.manageCookieAcceptanceForm.consentRadioButton,
    ).toBeChecked();

    // Change preference to "Rejected"
    await cookiesPolicyPage.manageCookieAcceptanceForm.rejectRadioButton.click();
    await cookiesPolicyPage.manageCookieAcceptanceForm.submitButton.click();

    // This makes the test resilient to whether you are logged in or not.
    await page.waitForURL('**/manage-your-appointments/**');

    // Use a polling expect to give the cookie time to update
    await expect
      .poll(
        async () => {
          const cookies = await page.context().cookies();
          const consentCookie = cookies.find(
            c => c.name === 'nhsuk-mya-cookie-consent',
          );
          return consentCookie?.value;
        },
        {
          message: 'Wait for cookie to be updated to false',
          intervals: [500, 1000],
          timeout: 5000,
        },
      )
      .toBe(
        '%257B%2522consented%2522%253Afalse%252C%2522version%2522%253A1%257D',
      );

    await expect(
      cookiesPolicyPage.cookieBanner.postAcceptanceMessage,
    ).toBeVisible();
  });
});
