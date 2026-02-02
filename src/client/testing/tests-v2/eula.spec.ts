import { test, expect } from '../fixtures-v2';
import EulaConsentPage from '../page-objects-v2/eula/eula-consent-page';

test.describe.configure({ mode: 'serial' });

test('A user with an out of date EULA consent version is prompted with the EULA consent page', async ({
  page,
  setUpSingleSite,
}) => {
  await setUpSingleSite()
    .then(async () => {
      const eulaConsentPage = new EulaConsentPage(page);
      
      await page.waitForURL('**/eula');
      await expect(eulaConsentPage.title).toBeVisible();

      // Try to bypass EULA consent by navigating to sites
      await page.goto('/manage-your-appointments/sites');

      // Assert redirected back to EULA
      await page.waitForURL('**/eula');
      await expect(eulaConsentPage.title).toBeVisible();
    });
});

test('A user with an out of date EULA version is prompted with the EULA consent page on login, but not again after they have consented', async ({
  page,
  setUpSingleSite,
}) => {
  const eulaConsentPage = new EulaConsentPage(page);

  await setUpSingleSite()
    .then(async () => {
      await page.waitForURL('**/eula');
      await expect(eulaConsentPage.title).toBeVisible();

      // Use the POM method to accept and return the next page
      return eulaConsentPage.acceptEula();
    })
    .then(async siteSelectionPage => {
      // Verify we reached site selection
      await expect(siteSelectionPage.title).toBeVisible();

      await page.goto('/');
      await page.waitForURL('**/sites');
      await expect(siteSelectionPage.title).toBeVisible();
    });
});