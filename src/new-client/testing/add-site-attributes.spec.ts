import { test, expect } from './fixtures';
import env from './testEnvironment';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import SiteSelectionPage from './page-objects/site-selection';
import SitePage from './page-objects/site';
import SiteManagementPage from './page-objects/site-management';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let siteManagementPage: SiteManagementPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  siteManagementPage = new SiteManagementPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.siteManagementCard.click();

  await page.waitForURL('**/site/ABC02/attributes');
});

test.only('Update access attributes for a site', async () => {
  // Check start state
  await siteManagementPage.attributeNotChecked('Accessible toilet');
  await siteManagementPage.attributeChecked('Step free access');

  // Toggle selected attributes
  await siteManagementPage.selectAttribute('Accessible toilet');
  await siteManagementPage.selectAttribute('Step free access');
  await siteManagementPage.confirmSiteDetailsButton.click();

  // Check banner function
  await expect(siteManagementPage.updateNotificationBanner).toBeVisible();
  await siteManagementPage.closeNotificationBannerButton.click();
  await expect(siteManagementPage.updateNotificationBanner).not.toBeVisible();

  // Check selected attributes have been toggled
  await siteManagementPage.attributeChecked('Accessible toilet');
  await siteManagementPage.attributeNotChecked('Step free access');

  // Reload page
  await siteManagementPage.page.reload();

  // Check selected attributes are still correctly toggled after page reload
  await siteManagementPage.attributeChecked('Accessible toilet');
  await siteManagementPage.attributeNotChecked('Step free access');
});
