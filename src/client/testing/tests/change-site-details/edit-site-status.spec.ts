import {
  RootPage,
  OAuthLoginPage,
  SiteSelectionPage,
  SitePage,
  SiteDetailsPage,
} from '@testing-page-objects';
import { Site } from '@types';
import EditSiteStatusPage from '../../page-objects/change-site-details-pages/edit-site-status';
import { test, overrideFeatureFlag } from '../../fixtures';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let siteStatusPage: EditSiteStatusPage;
let siteDetailsPage: SiteDetailsPage;

let site: Site;

test.describe.configure({ mode: 'serial' });

test.beforeAll(async () => {
  await overrideFeatureFlag('SiteStatus', true);
});

test.afterAll(async () => {
  await overrideFeatureFlag('SiteStatus', false);
});

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  siteStatusPage = new EditSiteStatusPage(page);
  siteDetailsPage = new SiteDetailsPage(page, site);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site);
  await sitePage.siteManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.changeSiteStatusButton.click();
  await page.waitForURL(`**/site/${site.id}/details/edit-site-status`);
});

test('Clicking back mid-form does not save the changes', async ({ page }) => {
  verifySummaryListItem(siteStatusPage.siteStatusLabel, 'Online');
  await siteStatusPage.takeSiteOffline.click();
  await siteStatusPage.backLink.click();

  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.verifySiteStatusNotificationVisibility(
    false,
    site.status ?? 'Online',
  );

  await siteDetailsPage.changeSiteStatusButton.click();
  await page.waitForURL(`**/site/${site.id}/details/edit-site-status`);

  verifySummaryListItem(siteStatusPage.siteStatusLabel, 'Online');
});

test('Update site status', async ({ page }) => {
  verifySummaryListItem(siteStatusPage.siteStatusLabel, 'Online');
  await siteStatusPage.takeSiteOffline.click();
  await siteStatusPage.saveAndContinueButton.click();

  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.verifySiteStatusNotificationVisibility(true, 'Offline');
});
