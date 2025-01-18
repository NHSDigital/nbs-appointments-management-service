import { test, expect } from '@playwright/test';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import SiteSelectionPage from './page-objects/site-selection';
import SitePage from './page-objects/site';
import CreateAvailabilityPage from './page-objects/create-availability';
import SummaryPage from './page-objects/create-availability/summary-page';
import { getDateInFuture } from './utils/date-utility';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let createAvailabilityPage: CreateAvailabilityPage;
let summarypage: SummaryPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  createAvailabilityPage = new CreateAvailabilityPage(page);
  summarypage = new SummaryPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.createAvailabilityCard.click();
  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/create-availability',
  );
});

test('A user can navigate to the Create Availability flow from the site page', async () => {
  await expect(createAvailabilityPage.title).toBeVisible();
});

test('Create single session of RSV availability', async () => {
  const tomorrowDate = getDateInFuture(1);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Single date session');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterSingleDateSessionDate(
    tomorrowDate.day,
    tomorrowDate.month,
    tomorrowDate.year,
  );
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterStartTime('09', '00');
  await createAvailabilityPage.enterEndTime('10', '00');
  await createAvailabilityPage.enterNoOfVaccinators('2');
  await createAvailabilityPage.appointmentLength('6');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.addService('RSV (Adult)');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.saveSessionButton.click();
  await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
});

test('Create weekly session of RSV availability', async () => {
  const tomorrowDate = getDateInFuture(1);
  const dayAfterTomorrowDate = getDateInFuture(2);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Weekly sessions');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterWeeklySessionStartDate(
    tomorrowDate.day,
    tomorrowDate.month,
    tomorrowDate.year,
  );
  await createAvailabilityPage.enterWeeklySessionEndDate(
    dayAfterTomorrowDate.day,
    dayAfterTomorrowDate.month,
    dayAfterTomorrowDate.year,
  );
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
  await createAvailabilityPage.saveSessionButton.click();
  await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
});

test('A user can navigate to the Create Availability flow validating weekly Session end date must be within the next year error', async () => {
  const tomorrowDate = getDateInFuture(1);
  const MoreThanAnYearDate = getDateInFuture(366);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Weekly sessions');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterWeeklySessionStartDate(
    tomorrowDate.day,
    tomorrowDate.month,
    tomorrowDate.year,
  );
  await createAvailabilityPage.enterWeeklySessionEndDate(
    MoreThanAnYearDate.day,
    MoreThanAnYearDate.month,
    MoreThanAnYearDate.year,
  );
  await createAvailabilityPage.continueButton.click();
  await expect(createAvailabilityPage.sessionEndDateErrorMsg).toBeVisible();
});

test('Create weekly session of RSV availability check sumary page links', async () => {
  const tomorrowDate = getDateInFuture(1);
  const dayAfterTomorrowDate = getDateInFuture(2);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Weekly sessions');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterWeeklySessionStartDate(
    tomorrowDate.day,
    tomorrowDate.month,
    tomorrowDate.year,
  );
  await createAvailabilityPage.enterWeeklySessionEndDate(
    dayAfterTomorrowDate.day,
    dayAfterTomorrowDate.month,
    dayAfterTomorrowDate.year,
  );
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

  // Then check Date change link is working
  await summarypage.changeFunctionalityLink('Date');
  await createAvailabilityPage.continueButton.click();

  // Then check Days change link is working
  await summarypage.changeFunctionalityLink('Days');
  await createAvailabilityPage.continueButton.click();

  // Then check Time change link is working
  await summarypage.changeFunctionalityLink('Time');
  await createAvailabilityPage.continueButton.click();

  // Then check vaccinators change link is working
  await summarypage.changeFunctionalityLink(
    'Vaccinators or vaccination spaces available',
  );
  await createAvailabilityPage.continueButton.click();

  // Then check Appointment change link is working
  await summarypage.changeFunctionalityLink('Appointment length');
  await createAvailabilityPage.continueButton.click();

  // Then check Services available change link is working
  await summarypage.changeFunctionalityLink('Services available');
  await createAvailabilityPage.continueButton.click();
});

test('Create single session of RSV availability check sumary page links', async () => {
  const tomorrowDate = getDateInFuture(1);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Single date session');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterSingleDateSessionDate(
    tomorrowDate.day,
    tomorrowDate.month,
    tomorrowDate.year,
  );
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterStartTime('09', '00');
  await createAvailabilityPage.enterEndTime('10', '00');
  await createAvailabilityPage.enterNoOfVaccinators('2');
  await createAvailabilityPage.appointmentLength('6');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.addService('RSV (Adult)');
  await createAvailabilityPage.continueButton.click();

  // Then check Date change link is working
  await summarypage.changeFunctionalityLink('Date');
  await createAvailabilityPage.continueButton.click();

  // Then check Time change link is working
  await summarypage.changeFunctionalityLink('Time');
  await createAvailabilityPage.continueButton.click();

  // Then check vaccinators change link is working
  await summarypage.changeFunctionalityLink(
    'Vaccinators or vaccination spaces available',
  );
  await createAvailabilityPage.continueButton.click();

  // Then check Appointment change link is working
  await summarypage.changeFunctionalityLink('Appointment length');
  await createAvailabilityPage.continueButton.click();

  // Then check Services available change link is working
  await summarypage.changeFunctionalityLink('Services available');
  await createAvailabilityPage.continueButton.click();
});
