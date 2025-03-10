import { test } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import SiteDetailsPage from '../../page-objects/change-site-details-pages/site-details';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let siteDetailsPage: SiteDetailsPage;

test.beforeEach(async ({ page, getTestSite }) => {
  const defaultSite = getTestSite();
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  siteDetailsPage = new SiteDetailsPage(page, defaultSite);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(defaultSite.name);
  await sitePage.siteManagementCard.click();
  await page.waitForURL(`**/site/${defaultSite.id}/details`);
});

test('Verify default information on site page', async () => {
  await siteDetailsPage.verifySitePage();
});
