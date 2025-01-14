import { test, expect } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import SiteManagementPage from '../../page-objects/site-management';
import SiteDetailsPage from '../../page-objects/site-details';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let siteManagementPage: SiteManagementPage;
let siteDetailsPage: SiteDetailsPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  siteManagementPage = new SiteManagementPage(page);
  siteDetailsPage = new SiteDetailsPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.siteManagementCard.click();
  await page.waitForURL('**/site/ABC02/details');
  await siteDetailsPage.editSiteAttributesButton.click();

  await page.waitForURL('**/site/ABC02/details/edit-attributes');
});

test('Update access attributes for a site', async ({ page }) => {
  // Toggle selected attributes
  await siteManagementPage.selectAttribute('Accessible toilet');
  await siteManagementPage.selectAttribute('Step free access');
  await siteManagementPage.confirmSiteDetailsButton.click();

  // Check banner function
  await expect(siteManagementPage.updateNotificationBanner).toBeVisible();
  await siteManagementPage.closeNotificationBannerButton.click();
  await expect(siteManagementPage.updateNotificationBanner).not.toBeVisible();

  // Go back into edit UI to assert on checkbox state:
  await siteDetailsPage.editSiteAttributesButton.click();
  await page.waitForURL('**/site/ABC02/details/edit-attributes');

  await siteManagementPage.attributeChecked('Accessible toilet');
  await siteManagementPage.attributeNotChecked('Step free access');

  // Reload page
  await siteManagementPage.page.reload();

  // Check selected attributes are still correctly toggled after page reload
  await siteManagementPage.attributeChecked('Accessible toilet');
  await siteManagementPage.attributeNotChecked('Step free access');
});
