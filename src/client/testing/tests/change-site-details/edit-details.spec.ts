import { test, expect } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import EditDetailsPage from '../../page-objects/change-site-details-pages/edit-details';
import SiteDetailsPage from '../../page-objects/change-site-details-pages/site-details';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let editDetailsPage: EditDetailsPage;
let siteDetailsPage: SiteDetailsPage;

let site: Site;

// Annotate entire file as serial.
test.describe.configure({ mode: 'serial' });

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  editDetailsPage = new EditDetailsPage(page);
  siteDetailsPage = new SiteDetailsPage(page, site);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site.name);
  await sitePage.siteManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.editSiteDetailsButton.click();

  await page.waitForURL(`**/site/${site.id}/details/edit-details`);
});

test('Clicking back mid-form does not save the changes', async ({ page }) => {
  await editDetailsPage.addressInput.fill('One House,\nOne Road,\nOne Town');
  await editDetailsPage.latitudeInput.fill('0.32445345');
  await editDetailsPage.longitudeInput.fill('53.742');
  await editDetailsPage.phoneNumberInput.fill('01189998819991197253');
  await editDetailsPage.backLink.click();

  //verify the data is NOT present on the new details
  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.verifyDetailsNotificationVisibility(false);

  //verify default state
  await siteDetailsPage.verifySitePage();

  // Go back into edit UI to assert input is same as before
  await siteDetailsPage.editSiteDetailsButton.click();
  await page.waitForURL(`**/site/${site.id}/details/edit-details`);

  //assert address is formed over multiple lines after page reload
  await expect(editDetailsPage.addressInput).toHaveValue(
    'Pudsey,\nLeeds,\nLS28 7LD',
  );
  await expect(editDetailsPage.latitudeInput).toHaveValue('-1.66382134');
  await expect(editDetailsPage.longitudeInput).toHaveValue('53.79628754');
  await expect(editDetailsPage.phoneNumberInput).toHaveValue('0113 2222222');

  await editDetailsPage.page.reload();
  await page.waitForURL(`**/site/${site.id}/details/edit-details`);

  //assert address is formed over multiple lines after page reload
  await expect(editDetailsPage.addressInput).toHaveValue(
    'Pudsey,\nLeeds,\nLS28 7LD',
  );
  await expect(editDetailsPage.latitudeInput).toHaveValue('-1.66382134');
  await expect(editDetailsPage.longitudeInput).toHaveValue('53.79628754');
  await expect(editDetailsPage.phoneNumberInput).toHaveValue('0113 2222222');
});

test('Update details for a site, and then reset', async ({ page }) => {
  await editDetailsPage.addressInput.fill('One House,\nOne Road,\nOne Town');
  await editDetailsPage.latitudeInput.fill('0.32445345');
  await editDetailsPage.longitudeInput.fill('53.742');
  await editDetailsPage.phoneNumberInput.fill('0118 999 88199 9119 725 3');
  await editDetailsPage.saveAndContinueButton.click();

  //verify the data is present on the new details
  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.verifyDetailsNotificationVisibility(true);

  await siteDetailsPage.verifyCoreDetailsContent(
    'One House, One Road, One Town',
    '0.32445345',
    '53.742',
    '0118 999 88199 9119 725 3',
  );

  // Go back into edit UI to assert new input is there
  await siteDetailsPage.editSiteDetailsButton.click();
  await page.waitForURL(`**/site/${site.id}/details/edit-details`);

  //assert address is formed over multiple lines after page reload
  await expect(editDetailsPage.addressInput).toHaveValue(
    'One House,\nOne Road,\nOne Town',
  );
  await expect(editDetailsPage.latitudeInput).toHaveValue('0.32445345');
  await expect(editDetailsPage.longitudeInput).toHaveValue('53.742');
  await expect(editDetailsPage.phoneNumberInput).toHaveValue(
    '0118 999 88199 9119 725 3',
  );

  await editDetailsPage.page.reload();
  await page.waitForURL(`**/site/${site.id}/details/edit-details`);

  //assert address is formed over multiple lines after page reload
  await expect(editDetailsPage.addressInput).toHaveValue(
    'One House,\nOne Road,\nOne Town',
  );
  await expect(editDetailsPage.latitudeInput).toHaveValue('0.32445345');
  await expect(editDetailsPage.longitudeInput).toHaveValue('53.742');
  await expect(editDetailsPage.phoneNumberInput).toHaveValue(
    '0118 999 88199 9119 725 3',
  );
});
