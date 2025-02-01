import { test, expect } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import EditAccessNeedsPage from '../../page-objects/change-site-details-pages/edit-access-need';
import SiteDetailsPage from '../../page-objects/change-site-details-pages/site-details';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let editAccessNeedsPage: EditAccessNeedsPage;
let siteDetailsPage: SiteDetailsPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  editAccessNeedsPage = new EditAccessNeedsPage(page);
  siteDetailsPage = new SiteDetailsPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.siteManagementCard.click();
  await page.waitForURL('**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details');
  await siteDetailsPage.editSiteAttributesButton.click();

  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-attributes',
  );
});

test('Update access attributes for a site', async ({ page }) => {
  // Toggle selected attributes
  await editAccessNeedsPage.selectAttribute('Accessible toilet');
  await editAccessNeedsPage.selectAttribute('Step free access');
  await editAccessNeedsPage.confirmSiteDetailsButton.click();

  // Check banner function
  await expect(editAccessNeedsPage.updateNotificationBanner).toBeVisible();
  await editAccessNeedsPage.closeNotificationBannerButton.click();
  await expect(editAccessNeedsPage.updateNotificationBanner).not.toBeVisible();

  // Go back into edit UI to assert on checkbox state:
  await siteDetailsPage.editSiteAttributesButton.click();
  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-attributes',
  );

  await editAccessNeedsPage.attributeChecked('Accessible toilet');
  await editAccessNeedsPage.attributeNotChecked('Step free access');

  // Reload page
  await editAccessNeedsPage.page.reload();
  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-attributes',
  );

  // Check selected attributes are still correctly toggled after page reload
  await editAccessNeedsPage.verifyAccessNeedsCheckedOrUnchecked(
    'Accessible toilet',
    'Checked',
  );
  await editAccessNeedsPage.verifyAccessNeedsCheckedOrUnchecked(
    'Step free access',
    'UnChecked',
  );

  //revert to default state
  await editAccessNeedsPage.selectAttribute('Accessible toilet');
  await editAccessNeedsPage.selectAttribute('Step free access');
  await editAccessNeedsPage.confirmSiteDetailsButton.click();
});
