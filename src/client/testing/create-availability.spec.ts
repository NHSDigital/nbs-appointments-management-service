import { test, expect } from '@playwright/test';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import SiteSelectionPage from './page-objects/site-selection';
import SitePage from './page-objects/site';
import CreateAvailabilityPage from './page-objects/create-availability';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let createAvailabilityPage: CreateAvailabilityPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  createAvailabilityPage = new CreateAvailabilityPage(page);
});

test('A user can navigate to the Create Availability flow from the site page', async ({
  page,
}) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.createAvailabilityCard.click();

  await page.waitForURL('**/site/ABC02/create-availability');
  await expect(createAvailabilityPage.title).toBeVisible();
});

test('Create single session of RSV availability', async ({ page }) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.createAvailabilityCard.click();
  await page.waitForURL('**/site/ABC02/create-availability');
  await createAvailabilityPage.CreateAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Single date session');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterSingleDateSessionDate('27', '10', '2025');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterStartTime('09', '00');
  await createAvailabilityPage.enterEndTime('10', '00');
  await createAvailabilityPage.enterNoOfVaccinators('1');
  await createAvailabilityPage.appointmentLength('5');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.addService('RSV (Adult)');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.btnSaveSession.click();
  await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
});

test('Create weekly session of RSV availability', async ({ page }) => {
  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.createAvailabilityCard.click();
  await createAvailabilityPage.CreateAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Weekly sessions');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterWeeklySessionStartDate('27', '10', '2025');
  await createAvailabilityPage.enterWeeklySessionEndDate('28', '10', '2025');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.selectDay('Select all days');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterStartTime('09', '00');
  await createAvailabilityPage.enterEndTime('10', '00');
  await createAvailabilityPage.enterNoOfVaccinators('1');
  await createAvailabilityPage.appointmentLength('5');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.addService('RSV (Adult)');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.btnSaveSession.click();
  await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
});
