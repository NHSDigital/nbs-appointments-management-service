import { test, expect } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import EditAccessNeedsPage from '../../page-objects/change-site-details-pages/edit-access-need';
import SiteDetailsPage from '../../page-objects/change-site-details-pages/site-details';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let editAccessNeedstPage: EditAccessNeedsPage;
let siteDetailsPage: SiteDetailsPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  editAccessNeedstPage = new EditAccessNeedsPage(page);
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
  await editAccessNeedstPage.selectAttribute('Accessible toilet');
  await editAccessNeedstPage.selectAttribute('Step free access');
  await editAccessNeedstPage.confirmSiteDetailsButton.click();

  // Check banner function
  await expect(editAccessNeedstPage.updateNotificationBanner).toBeVisible();
  await editAccessNeedstPage.closeNotificationBannerButton.click();
  await expect(editAccessNeedstPage.updateNotificationBanner).not.toBeVisible();

  // Go back into edit UI to assert on checkbox state:
  await siteDetailsPage.editSiteAttributesButton.click();
  await page.waitForURL('**/site/ABC02/details/edit-attributes');

  await editAccessNeedstPage.attributeChecked('Accessible toilet');
  await editAccessNeedstPage.attributeNotChecked('Step free access');

  // Reload page
  await editAccessNeedstPage.page.reload();
  await page.waitForURL('**/site/ABC02/details/edit-attributes');

  // Check selected attributes are still correctly toggled after page reload
  await editAccessNeedstPage.verifyAccessNeedsCheckedOrUnchecked(
    'Accessible toilet',
    'Checked',
  );
  await editAccessNeedstPage.verifyAccessNeedsCheckedOrUnchecked(
    'Step free access',
    'UnChecked',
  );
});
