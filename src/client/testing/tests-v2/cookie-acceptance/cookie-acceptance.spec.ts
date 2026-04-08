import { test, expect } from '../../fixtures-v2';
import { CookiePoliciesPage } from '@e2etests/page-objects';

let cookiesPage: CookiePoliciesPage;

test.beforeEach(async ({ page }) => {
  cookiesPage = new CookiePoliciesPage(page);
  await page.context().clearCookies();
});

test('The user accepts analytics cookies', async ({ page, setup }) => {
  await setup({}).then(async () => {
    await expect(cookiesPage.cookieBanner.preAcceptanceHeader).toBeVisible();

    await cookiesPage.cookieBanner.acceptCookiesButton.click();

    await expect(
      cookiesPage.cookieBanner.preAcceptanceHeader,
    ).not.toBeVisible();
    await expect(cookiesPage.cookieBanner.postAcceptanceMessage).toBeVisible();

    const cookies = await page.context().cookies();
    const consentCookie = cookies.find(
      c => c.name === 'nhsuk-mya-cookie-consent',
    );

    expect(consentCookie).toBeTruthy();
    expect(consentCookie?.value).toContain('consented%2522%253Atrue');
  });
});

test('The user rejects analytics cookies', async ({ page, setup }) => {
  await setup({}).then(async () => {
    await expect(cookiesPage.cookieBanner.preAcceptanceHeader).toBeVisible();

    await cookiesPage.cookieBanner.rejectCookiesButton.click();

    await expect(
      cookiesPage.cookieBanner.preAcceptanceHeader,
    ).not.toBeVisible();
    await expect(cookiesPage.cookieBanner.postAcceptanceMessage).toBeVisible();

    const cookies = await page.context().cookies();
    const consentCookie = cookies.find(
      c => c.name === 'nhsuk-mya-cookie-consent',
    );

    expect(consentCookie).toBeTruthy();
    expect(consentCookie?.value).toContain('consented%2522%253Afalse');
  });
});

test('The banner only shows when preferences are not set', async ({
  page,
  setup,
}) => {
  await setup({}).then(async () => {
    await expect(cookiesPage.cookieBanner.preAcceptanceHeader).toBeVisible();
    await cookiesPage.cookieBanner.acceptCookiesButton.click();
    await expect(cookiesPage.cookieBanner.postAcceptanceMessage).toBeVisible();
    await page.reload();

    // Banner should not reappear after refresh
    await expect(
      cookiesPage.cookieBanner.preAcceptanceHeader,
    ).not.toBeVisible();

    await page.context().clearCookies();
    await page.reload();

    // Banner should return once cookies are gone
    await expect(cookiesPage.cookieBanner.preAcceptanceHeader).toBeVisible();
  });
});
