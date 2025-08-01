import {
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
} from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;

let site: Site;

test.beforeEach(async ({ page, getTestSite, getTestUser }) => {
  site = getTestSite(1);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(getTestUser(12));
});

test('Navigates to the reports page via the header before selecting a site', async () => {
  const reportsPage = await siteSelectionPage.topNav.clickReports();

  await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
});

test('Navigates to the reports page via a site page', async () => {
  await siteSelectionPage.selectSite(site);
  const reportsPage = await sitePage.clickReportsCard();

  await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
});

test('Downloads a site summary report', async () => {
  const reportsPage = await siteSelectionPage.topNav.clickReports();

  // TODO: Select two dates using the date pickers

  await reportsPage.selectDatesStep.continueButton.click();
  await expect(reportsPage.confirmDownloadStep.stepTitle).toBeVisible();
  await reportsPage.confirmDownloadStep.continueButton.click();

  // TODO: Assert file download starts?
});
