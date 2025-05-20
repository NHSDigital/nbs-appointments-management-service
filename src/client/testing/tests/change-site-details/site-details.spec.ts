import { test } from '../../fixtures';
import {
  OAuthLoginPage,
  RootPage,
  SiteDetailsPage,
  SitePage,
  SiteSelectionPage,
} from '@testing-page-objects';

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
  await siteSelectionPage.selectSite(defaultSite);
  await sitePage.siteManagementCard.click();
  await page.waitForURL(`**/site/${defaultSite.id}/details`);
});

test('Verify default information on site page', async () => {
  await siteDetailsPage.verifySitePage();
});
