import { test, expect } from '@playwright/test';
import RootPage from './page-objects/root';
import OAuthLoginPage from './page-objects/oauth';
import SiteSelectionPage from './page-objects/site-selection';
import SitePage from './page-objects/site';
import DateUtils from './utils/date-utility';
import CreateAvailabilityPage from './page-objects/create-availability';
import SummaryPage from './page-objects/create-availability/summary-page';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let createAvailabilityPage: CreateAvailabilityPage;
let dateUtils: DateUtils;
let summarypage: SummaryPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  createAvailabilityPage = new CreateAvailabilityPage(page);
  dateUtils = new DateUtils();
  summarypage = new SummaryPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.createAvailabilityCard.click();
  await page.waitForURL('**/site/ABC02/create-availability');
});

test('A user can navigate to the Create Availability flow from the site page', async ({
  page,
}) => {
  await expect(createAvailabilityPage.title).toBeVisible();
});

test('Create single session of RSV availability', async ({ page }) => {
  const tomorrowDate = dateUtils.getFutureDate(1);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Single date session');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterSingleDateSessionDate(
    tomorrowDate[2],
    tomorrowDate[1],
    tomorrowDate[0],
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

test('Create weekly session of RSV availability', async ({ page }) => {
  const tomorrowDate = dateUtils.getFutureDate(1);
  const dayAfterTomorrowDate = dateUtils.getFutureDate(2);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Weekly sessions');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterWeeklySessionStartDate(
    tomorrowDate[2],
    tomorrowDate[1],
    tomorrowDate[0],
  );
  await createAvailabilityPage.enterWeeklySessionEndDate(
    dayAfterTomorrowDate[2],
    dayAfterTomorrowDate[1],
    dayAfterTomorrowDate[0],
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

test('A user can navigate to the Create Availability flow validating weekly Session end date must be within the next year error', async ({
  page,
}) => {
  const tomorrowDate = dateUtils.getFutureDate(1);
  const MoreThanAnYearDate = dateUtils.getFutureDate(366);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Weekly sessions');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterWeeklySessionStartDate(
    tomorrowDate[2],
    tomorrowDate[1],
    tomorrowDate[0],
  );
  await createAvailabilityPage.enterWeeklySessionEndDate(
    MoreThanAnYearDate[2],
    MoreThanAnYearDate[1],
    MoreThanAnYearDate[0],
  );
  await createAvailabilityPage.continueButton.click();
  await expect(createAvailabilityPage.sessionEndDateErrorMsg).toBeVisible();
});

test('Create weekly session of RSV availability check sumary page links', async ({
  page,
}) => {
  const tomorrowDate = dateUtils.getFutureDate(1);
  const dayAfterTomorrowDate = dateUtils.getFutureDate(2);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Weekly sessions');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterWeeklySessionStartDate(
    tomorrowDate[2],
    tomorrowDate[1],
    tomorrowDate[0],
  );
  await createAvailabilityPage.enterWeeklySessionEndDate(
    dayAfterTomorrowDate[2],
    dayAfterTomorrowDate[1],
    dayAfterTomorrowDate[0],
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

test('Create single session of RSV availability check sumary page links', async ({
  page,
}) => {
  const tomorrowDate = dateUtils.getFutureDate(1);
  await createAvailabilityPage.createAvailabilityButton.click();
  await expect(createAvailabilityPage.sessionTitle).toBeVisible();
  await createAvailabilityPage.selectSession('Single date session');
  await createAvailabilityPage.continueButton.click();
  await createAvailabilityPage.enterSingleDateSessionDate(
    tomorrowDate[2],
    tomorrowDate[1],
    tomorrowDate[0],
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
