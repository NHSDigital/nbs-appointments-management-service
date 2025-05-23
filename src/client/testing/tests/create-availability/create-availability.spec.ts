import {
  CreateAvailabilityPage,
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
  SummaryPage,
} from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import { getDateInFuture } from '../../utils/date-utility';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let createAvailabilityPage: CreateAvailabilityPage;
let summarypage: SummaryPage;
let site: Site;

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      test.describe('Create Availability', () => {
        test.beforeEach(async ({ page, getTestSite }) => {
          site = getTestSite();
          rootPage = new RootPage(page);
          oAuthPage = new OAuthLoginPage(page);
          siteSelectionPage = new SiteSelectionPage(page);
          sitePage = new SitePage(page);
          createAvailabilityPage = new CreateAvailabilityPage(page);
          summarypage = new SummaryPage(page);

          await rootPage.goto();
          await rootPage.pageContentLogInButton.click();
          await oAuthPage.signIn();
          await siteSelectionPage.selectSite(site);
          await sitePage.createAvailabilityCard.click();
          await page.waitForURL(`**/site/${site.id}/create-availability`);
        });

        test('A user can navigate to the Create Availability flow from the site page', async () => {
          await expect(createAvailabilityPage.title).toBeVisible();
        });

        test('Create single session of RSV availability', async ({ page }) => {
          const tomorrowDate = getDateInFuture(1);
          await createAvailabilityPage.createAvailabilityButton.click();
          await page.waitForURL(
            `**/site/${site.id}/create-availability/wizard`,
          );

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
          await createAvailabilityPage.addService('RSV Adult');
          await createAvailabilityPage.continueButton.click();
          await createAvailabilityPage.saveSessionButton.click();
          await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
        });

        test('Create weekly session of RSV availability', async ({ page }) => {
          const tomorrowDate = getDateInFuture(1);
          const dayAfterTomorrowDate = getDateInFuture(2);
          await createAvailabilityPage.createAvailabilityButton.click();
          await page.waitForURL(
            `**/site/${site.id}/create-availability/wizard`,
          );

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
          await createAvailabilityPage.addService('RSV Adult');
          await createAvailabilityPage.continueButton.click();
          await createAvailabilityPage.saveSessionButton.click();
          await expect(createAvailabilityPage.sessionSuccessMsg).toBeVisible();
        });

        test('A user can navigate to the Create Availability flow validating weekly Session start date must be within the next year error', async () => {
          const moreThanAnYearDate = getDateInFuture(366);
          await createAvailabilityPage.createAvailabilityButton.click();
          await expect(createAvailabilityPage.sessionTitle).toBeVisible();
          await createAvailabilityPage.selectSession('Weekly sessions');
          await createAvailabilityPage.continueButton.click();
          await createAvailabilityPage.enterWeeklySessionStartDate(
            moreThanAnYearDate.day,
            moreThanAnYearDate.month,
            moreThanAnYearDate.year,
          );
          await createAvailabilityPage.enterWeeklySessionEndDate(
            moreThanAnYearDate.day,
            moreThanAnYearDate.month,
            moreThanAnYearDate.year,
          );
          await createAvailabilityPage.continueButton.click();
          await expect(
            createAvailabilityPage.sessionStartDateErrorMsg,
          ).toBeVisible();
          await expect(
            createAvailabilityPage.sessionEndDateErrorMsg,
          ).toBeVisible();
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
          await expect(
            createAvailabilityPage.sessionEndDateErrorMsg,
          ).toBeVisible();
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
          await createAvailabilityPage.addService('RSV Adult');
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
          await createAvailabilityPage.addService('RSV Adult');
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
      });
    });
  },
);
