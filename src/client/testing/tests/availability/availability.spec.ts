import {
  CreateAvailabilityPage,
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
  SummaryPage,
  AddServicesPage,
  AddSessionPage,
  CancelSessionDetailsPage,
  ChangeAvailabilityPage,
  CheckSessionDetailsPage,
  DailyAppointmentDetailsPage,
  EditAvailabilityConfirmedPage,
  MonthViewAvailabilityPage,
  WeekViewAvailabilityPage,
  CancelAppointmentDetailsPage,
} from '@testing-page-objects';
import {
  test,
  expect,
  overrideFeatureFlag,
  clearAllFeatureFlagOverrides,
  BrowserContext,
  Page,
} from '../../fixtures';
import { Site } from '@types';

import {
  daysFromToday,
  weekHeaderText,
  getDateInFuture,
} from '../../utils/date-utility';
import { parseToUkDatetime } from '@services/timeService';
import { sessionTestCases, weekTestCases } from '../../availability';

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
test.describe.configure({ mode: 'serial' });

[false, true].forEach(multipleServicesEnabled => {
  test.describe(`Availability Tests for MultipleServices enabled: '${multipleServicesEnabled}'`, () => {
    test.beforeAll(async () => {
      await overrideFeatureFlag('MultipleServices', multipleServicesEnabled);
    });

    test.afterAll(async () => {
      await clearAllFeatureFlagOverrides();
    });

    test.describe('Create Availability', () => {
      let rootPage: RootPage;
      let oAuthPage: OAuthLoginPage;
      let siteSelectionPage: SiteSelectionPage;
      let sitePage: SitePage;
      let createAvailabilityPage: CreateAvailabilityPage;
      let summarypage: SummaryPage;
      let site: Site;

      // ['Europe/London', 'UTC', 'Pacific/Kiritimati', 'Etc/GMT+12']
      ['Europe/London'].forEach(timezone => {
        test.describe(`Test in timezone: '${timezone}'`, () => {
          test.use({ timezoneId: timezone });

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

          test('Create single session of RSV availability', async ({
            page,
          }) => {
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const futureDate = getDateInFuture(dayIncrement);
            await createAvailabilityPage.createAvailabilityButton.click();
            await page.waitForURL(
              `**/site/${site.id}/create-availability/wizard`,
            );

            await expect(createAvailabilityPage.sessionTitle).toBeVisible();
            await createAvailabilityPage.selectSession('Single date session');
            await createAvailabilityPage.continueButton.click();
            await createAvailabilityPage.enterSingleDateSessionDate(
              futureDate.day,
              futureDate.month,
              futureDate.year,
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
            await expect(
              createAvailabilityPage.sessionSuccessMsg,
            ).toBeVisible();
          });

          test('Create weekly session of RSV availability', async ({
            page,
          }) => {
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const futureDate = getDateInFuture(dayIncrement);
            const dayAfterFutureDate = getDateInFuture(dayIncrement + 1);
            await createAvailabilityPage.createAvailabilityButton.click();
            await page.waitForURL(
              `**/site/${site.id}/create-availability/wizard`,
            );

            await expect(createAvailabilityPage.sessionTitle).toBeVisible();
            await createAvailabilityPage.selectSession('Weekly sessions');
            await createAvailabilityPage.continueButton.click();
            await createAvailabilityPage.enterWeeklySessionStartDate(
              futureDate.day,
              futureDate.month,
              futureDate.year,
            );
            await createAvailabilityPage.enterWeeklySessionEndDate(
              dayAfterFutureDate.day,
              dayAfterFutureDate.month,
              dayAfterFutureDate.year,
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
            await expect(
              createAvailabilityPage.sessionSuccessMsg,
            ).toBeVisible();
          });

          test('A user can navigate to the Create Availability flow validating weekly Session start date must be within the next year error', async () => {
            let dayIncrement = 366;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const moreThanAnYearDate = getDateInFuture(dayIncrement);
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
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const futureDate = getDateInFuture(dayIncrement);
            const aYearFutureDate = getDateInFuture(dayIncrement + 365);
            await createAvailabilityPage.createAvailabilityButton.click();
            await expect(createAvailabilityPage.sessionTitle).toBeVisible();
            await createAvailabilityPage.selectSession('Weekly sessions');
            await createAvailabilityPage.continueButton.click();
            await createAvailabilityPage.enterWeeklySessionStartDate(
              futureDate.day,
              futureDate.month,
              futureDate.year,
            );
            await createAvailabilityPage.enterWeeklySessionEndDate(
              aYearFutureDate.day,
              aYearFutureDate.month,
              aYearFutureDate.year,
            );
            await createAvailabilityPage.continueButton.click();
            await expect(
              createAvailabilityPage.sessionEndDateErrorMsg,
            ).toBeVisible();
          });

          test('Create weekly session of RSV availability check summary page links', async () => {
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const futureDate = getDateInFuture(dayIncrement);
            const dayAfterTomorrowDate = getDateInFuture(dayIncrement + 1);
            await createAvailabilityPage.createAvailabilityButton.click();
            await expect(createAvailabilityPage.sessionTitle).toBeVisible();
            await createAvailabilityPage.selectSession('Weekly sessions');
            await createAvailabilityPage.continueButton.click();
            await createAvailabilityPage.enterWeeklySessionStartDate(
              futureDate.day,
              futureDate.month,
              futureDate.year,
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

          test('Create single session of RSV availability check summary page links', async () => {
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const futureDate = getDateInFuture(dayIncrement);
            await createAvailabilityPage.createAvailabilityButton.click();
            await expect(createAvailabilityPage.sessionTitle).toBeVisible();
            await createAvailabilityPage.selectSession('Single date session');
            await createAvailabilityPage.continueButton.click();
            await createAvailabilityPage.enterSingleDateSessionDate(
              futureDate.day,
              futureDate.month,
              futureDate.year,
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
    });

    test.describe('Add Session', () => {
      let rootPage: RootPage;
      let oAuthPage: OAuthLoginPage;
      let siteSelectionPage: SiteSelectionPage;
      let sitePage: SitePage;
      let monthViewAvailabilityPage: MonthViewAvailabilityPage;
      let weekViewAvailabilityPage: WeekViewAvailabilityPage;
      let addSessionPage: AddSessionPage;
      let addServicesPage: AddServicesPage;
      let checkSessionDetailsPage: CheckSessionDetailsPage;
      let changeAvailabilityPage: ChangeAvailabilityPage;
      let editAvailabilityConfirmedPage: EditAvailabilityConfirmedPage;
      let cancelSessionDetailsPage: CancelSessionDetailsPage;
      let dailyAppointmentDetailsPage: DailyAppointmentDetailsPage;

      // ['Europe/London', 'UTC', 'Pacific/Kiritimati', 'Etc/GMT+12']
      ['Europe/London'].forEach(timezone => {
        test.describe(`Test in timezone: '${timezone}'`, () => {
          test.use({ timezoneId: timezone });

          test.beforeEach(async ({ page, getTestSite }) => {
            rootPage = new RootPage(page);
            oAuthPage = new OAuthLoginPage(page);
            siteSelectionPage = new SiteSelectionPage(page);
            sitePage = new SitePage(page);
            monthViewAvailabilityPage = new MonthViewAvailabilityPage(page);
            weekViewAvailabilityPage = new WeekViewAvailabilityPage(page);
            addSessionPage = new AddSessionPage(page);
            addServicesPage = new AddServicesPage(page);
            checkSessionDetailsPage = new CheckSessionDetailsPage(page);
            changeAvailabilityPage = new ChangeAvailabilityPage(page);
            cancelSessionDetailsPage = new CancelSessionDetailsPage(page);
            dailyAppointmentDetailsPage = new DailyAppointmentDetailsPage(page);
            editAvailabilityConfirmedPage = new EditAvailabilityConfirmedPage(
              page,
            );

            await rootPage.goto();
            await rootPage.cookieBanner.acceptCookiesButton.click();
            await rootPage.pageContentLogInButton.click();
            await oAuthPage.signIn();

            await siteSelectionPage.selectSite(getTestSite(2));
            await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
            await page.waitForURL('**/site/**/view-availability');
          });

          test('Verify user is able to add a session for future date', async ({
            page,
          }) => {
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
            const requiredWeekRange = weekHeaderText(
              daysFromToday(dayIncrement),
            );

            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              requiredDate,
            );
            await weekViewAvailabilityPage.addAvailability(requiredDate);

            await page.waitForURL(
              '**/site/**/create-availability/wizard?date=**',
            );

            await addSessionPage.verifyAddSessionPageDisplayed();
            await addSessionPage.addSession('9', '00', '10', '00', '1', '5');

            await addServicesPage.verifyAddServicesPageDisplayed();
            await addServicesPage.addService('RSV Adult');

            await checkSessionDetailsPage.verifyCheckSessionDetailsPageDisplayed();
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifySessionAdded();
          });

          test('Verify add availability option displayed for future date', async ({
            page,
          }) => {
            let dayIncrement = 2;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
            const requiredWeekRange = weekHeaderText(
              daysFromToday(dayIncrement),
            );
            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              requiredDate,
            );
            await weekViewAvailabilityPage.verifyAddAvailabilityButtonDisplayed(
              requiredDate,
            );
          });

          test('Verify user is able to change availability', async ({
            page,
          }) => {
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
            const requiredWeekRange = weekHeaderText(
              daysFromToday(dayIncrement),
            );

            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              requiredDate,
            );
            await weekViewAvailabilityPage.addAvailability(requiredDate);

            await page.waitForURL(
              '**/site/**/create-availability/wizard?date=**',
            );

            await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
            await addServicesPage.addService('RSV Adult');
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifySessionAdded();
            await weekViewAvailabilityPage.openChangeAvailabilityPage(
              requiredDate,
            );

            await page.waitForURL(
              '**/site/**/view-availability/week/edit-session?date=**',
            );

            await changeAvailabilityPage.selectChangeType(
              'ChangeLengthCapacity',
            );
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL('**/site/**/availability/edit?session=**');

            await addSessionPage.updateSessionEndTime('9', '30');

            await page.waitForURL(
              '**/site/**/availability/edit/confirmed?updatedSession=**',
            );
            await editAvailabilityConfirmedPage.verifySessionUpdated();
          });

          test('Verify user is able to cancel session', async ({ page }) => {
            let dayIncrement = 5;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
            const requiredWeekRange = weekHeaderText(
              daysFromToday(dayIncrement),
            );

            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              requiredDate,
            );
            await weekViewAvailabilityPage.addAvailability(requiredDate);

            await page.waitForURL(
              '**/site/**/create-availability/wizard?date=**',
            );

            await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
            await addServicesPage.addService('RSV Adult');
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifySessionAdded();
            await weekViewAvailabilityPage.openChangeAvailabilityPage(
              requiredDate,
            );

            await page.waitForURL(
              '**/site/**/view-availability/week/edit-session?date=**',
            );
            await changeAvailabilityPage.selectChangeType('CancelSession');
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL('**/site/**/availability/cancel?session=**');

            await cancelSessionDetailsPage.confirmSessionCancellation('Yes');

            await page.waitForURL(
              '**/site/**/availability/cancel/confirmed?session=**',
            );

            const cancelDate = daysFromToday(dayIncrement, 'DD MMMM');
            await cancelSessionDetailsPage.verifySessionCancelled(cancelDate);
          });

          test('Verify session not canceled if not confirmed', async ({
            page,
          }) => {
            let dayIncrement = 3;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
            const requiredWeekRange = weekHeaderText(
              daysFromToday(dayIncrement),
            );

            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              requiredDate,
            );
            await weekViewAvailabilityPage.addAvailability(requiredDate);

            await page.waitForURL(
              '**/site/**/create-availability/wizard?date=**',
            );

            await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
            await addServicesPage.addService('RSV Adult');
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifySessionAdded();
            await weekViewAvailabilityPage.openChangeAvailabilityPage(
              requiredDate,
            );

            await page.waitForURL(
              '**/site/**/view-availability/week/edit-session?date=**',
            );
            await changeAvailabilityPage.selectChangeType('CancelSession');
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL('**/site/**/availability/cancel?session=**');

            await cancelSessionDetailsPage.confirmSessionCancellation('No');
            await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
            await dailyAppointmentDetailsPage.navigateToWeekView();

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyFirstSessionRecordDetail(
              requiredDate,
              '09:00 - 10:00',
              'RSV Adult',
            );
          });

          test('Verify view daily appointment link displayed', async ({
            page,
          }) => {
            let dayIncrement = 3;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
            const requiredWeekRange = weekHeaderText(
              daysFromToday(dayIncrement),
            );

            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              requiredDate,
            );
            await weekViewAvailabilityPage.addAvailability(requiredDate);

            await page.waitForURL(
              '**/site/**/create-availability/wizard?date=**',
            );

            await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
            await addServicesPage.addService('RSV Adult');
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifySessionAdded();
            await weekViewAvailabilityPage.openDailyAppointmentPage(
              requiredDate,
            );
            await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
          });

          test('Verify appointment not cancelled when not confirmed', async ({
            page,
          }) => {
            const requiredDate = '2025-08-05';
            const formattedDate =
              parseToUkDatetime(requiredDate).format('dddd D MMMM');
            const requiredWeekRange = weekHeaderText(requiredDate);

            await monthViewAvailabilityPage.navigateToRequiredMonth(
              '6877d86e-c2df-4def-8508-e1eccf0ea6be',
              requiredDate,
            );
            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              formattedDate,
            );
            await weekViewAvailabilityPage.openDailyAppointmentPage(
              formattedDate,
            );

            await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
            await dailyAppointmentDetailsPage.cancelAppointment('5932817282');
            await dailyAppointmentDetailsPage.confirmAppointmentCancellation(
              'No',
            );
            await dailyAppointmentDetailsPage.verifyAppointmentNotCancelled(
              '5932817282',
            );
          });

          test('Verify availibility with no bookings is cancelled and manual appointments folder is empty', async ({
            page,
          }) => {
            let dayIncrement = 1;

            //avoid collisions
            if (multipleServicesEnabled) {
              dayIncrement += 7;
            }

            const requiredDate = daysFromToday(dayIncrement);
            const formattedDate1 =
              parseToUkDatetime(requiredDate).format('DD MMMM');
            const formattedDate2 =
              parseToUkDatetime(requiredDate).format('dddd D MMMM');
            const requiredWeekRange = weekHeaderText(
              daysFromToday(dayIncrement),
            );

            await monthViewAvailabilityPage.verifyViewMonthDisplayed(
              requiredWeekRange,
            );
            await monthViewAvailabilityPage.openWeekViewHavingDate(
              requiredWeekRange,
            );

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifyDateCardDisplayed(
              formattedDate2,
            );
            await weekViewAvailabilityPage.addAvailability(formattedDate2);

            await page.waitForURL(
              '**/site/**/create-availability/wizard?date=**',
            );

            await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
            await addServicesPage.addService('RSV Adult');
            await checkSessionDetailsPage.saveSession();

            await page.waitForURL('**/site/**/view-availability/week?date=**');

            await weekViewAvailabilityPage.verifySessionAdded();
            await weekViewAvailabilityPage.openChangeAvailabilityPage(
              formattedDate2,
            );

            await page.waitForURL(
              '**/site/**/view-availability/week/edit-session?date=**',
            );
            await changeAvailabilityPage.selectChangeType('CancelSession');
            await changeAvailabilityPage.saveChanges();

            await page.waitForURL('**/site/**/availability/cancel?session=**');

            await cancelSessionDetailsPage.confirmSessionCancellation('Yes');

            await page.waitForURL(
              '**/site/**/availability/cancel/confirmed?session=**',
            );

            await cancelSessionDetailsPage.verifySessionCancelled(
              formattedDate1,
            );
            await cancelSessionDetailsPage.clickCancelAppointment();

            await page.waitForURL(
              '**/site/**/view-availability/daily-appointments?date=**',
            );

            await dailyAppointmentDetailsPage.verifyManualAppointment();
          });
        });
      });
    });

    test.describe('View Month Availability', () => {
      let rootPage: RootPage;
      let oAuthPage: OAuthLoginPage;
      let monthViewAvailabilityPage: MonthViewAvailabilityPage;
      let site: Site;

      let context: BrowserContext;
      let page: Page;

      test.beforeAll(async ({ browser, getTestSite }) => {
        context = await browser.newContext();
        page = await context.newPage();

        site = getTestSite(2);
        rootPage = new RootPage(page);
        oAuthPage = new OAuthLoginPage(page);
        monthViewAvailabilityPage = new MonthViewAvailabilityPage(page);

        await rootPage.goto();
        await rootPage.pageContentLogInButton.click();
        await oAuthPage.signIn();
      });

      test.afterAll(async () => {
        await context.close();
      });

      // ['Europe/London', 'UTC', 'Pacific/Kiritimati', 'Etc/GMT+12']
      ['Europe/London'].forEach(timezone => {
        test.describe(`Test in timezone: '${timezone}'`, () => {
          test.use({ timezoneId: timezone });

          test('All the month page data is arranged in the week cards as expected - Oct 2025', async () => {
            //go to a specific month page that has a daylight savings change
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability?date=2025-10-20`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability?date=2025-10-20`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            await monthViewAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
              'September 2025',
              'November 2025',
            );
            await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
              [
                {
                  header: '29 September to 5 October',
                  services: [],
                  totalAppointments: 0,
                  booked: 0,
                  unbooked: 0,
                },
                {
                  header: '6 October to 12 October',
                  services: [],
                  totalAppointments: 0,
                  booked: 0,
                  unbooked: 0,
                },
                {
                  header: '13 October to 19 October',
                  services: [],
                  totalAppointments: 0,
                  booked: 0,
                  unbooked: 0,
                },
                {
                  header: '20 October to 26 October',
                  services: [
                    { serviceName: 'RSV Adult', bookedAppointments: 4 },
                  ],
                  totalAppointments: 840,
                  booked: 4,
                  unbooked: 836,
                },
                {
                  header: '27 October to 2 November',
                  services: [
                    { serviceName: 'RSV Adult', bookedAppointments: 2 },
                  ],
                  totalAppointments: 420,
                  booked: 2,
                  unbooked: 418,
                },
              ],
            );
          });

          test('All the month page data is arranged in the week cards as expected - March 2026', async () => {
            //go to a specific month page that has a daylight savings change
            await page.goto(
              `manage-your-appointments/site/${site.id}/view-availability?date=2026-03-01`,
            );
            await page.waitForURL(
              `**/site/${site.id}/view-availability?date=2026-03-01`,
            );
            await page.waitForSelector('.nhsuk-loader', {
              state: 'detached',
            });

            await monthViewAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
              'February 2026',
              'April 2026',
            );
            await monthViewAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
              [
                {
                  header: '23 February to 1 March',
                  services: [],
                  totalAppointments: 0,
                  booked: 0,
                  unbooked: 0,
                },
                {
                  header: '2 March to 8 March',
                  services: [],
                  totalAppointments: 0,
                  booked: 0,
                  unbooked: 0,
                },
                {
                  header: '9 March to 15 March',
                  services: [],
                  totalAppointments: 0,
                  booked: 0,
                  unbooked: 0,
                },
                {
                  header: '16 March to 22 March',
                  services: [],
                  totalAppointments: 0,
                  booked: 0,
                  unbooked: 0,
                },
                {
                  header: '23 March to 29 March',
                  services: [
                    { serviceName: 'RSV Adult', bookedAppointments: 4 },
                  ],
                  totalAppointments: 480,
                  booked: 4,
                  unbooked: 476,
                },
                {
                  header: '30 March to 5 April',
                  services: [
                    { serviceName: 'RSV Adult', bookedAppointments: 2 },
                  ],
                  totalAppointments: 240,
                  booked: 2,
                  unbooked: 238,
                },
              ],
            );
          });
        });
      });
    });

    test.describe('View Week Availability', () => {
      let rootPage: RootPage;
      let oAuthPage: OAuthLoginPage;
      let weekViewAvailabilityPage: WeekViewAvailabilityPage;
      let changeAvailabilityPage: ChangeAvailabilityPage;
      let addSessionPage: AddSessionPage;
      let cancelSessionDetailsPage: CancelSessionDetailsPage;
      let dailyAppointmentDetailsPage: DailyAppointmentDetailsPage;
      let cancelAppointmentDetailsPage: CancelAppointmentDetailsPage;
      let site: Site;

      let context: BrowserContext;
      let page: Page;

      test.beforeAll(async ({ browser, getTestSite }) => {
        context = await browser.newContext();
        page = await context.newPage();

        site = getTestSite(2);
        rootPage = new RootPage(page);
        oAuthPage = new OAuthLoginPage(page);
        weekViewAvailabilityPage = new WeekViewAvailabilityPage(page);
        changeAvailabilityPage = new ChangeAvailabilityPage(page);
        addSessionPage = new AddSessionPage(page);
        cancelSessionDetailsPage = new CancelSessionDetailsPage(page);
        dailyAppointmentDetailsPage = new DailyAppointmentDetailsPage(page);
        cancelAppointmentDetailsPage = new CancelAppointmentDetailsPage(page);

        await rootPage.goto();
        await rootPage.pageContentLogInButton.click();
        await oAuthPage.signIn();
      });

      test.afterAll(async () => {
        await context.close();
      });

      // ['Europe/London', 'UTC', 'Pacific/Kiritimati', 'Etc/GMT+12']
      ['Europe/London'].forEach(timezone => {
        test.describe(`Test in timezone: '${timezone}'`, () => {
          test.use({ timezoneId: timezone });

          weekTestCases.forEach(week => {
            test.describe(`Session tests for week: '${week.weekHeader}'`, () => {
              test.beforeEach(async () => {
                //start test by navigating to the week view that contains this session
                await page.goto(
                  `manage-your-appointments/site/${site.id}/view-availability/week?date=${week.week}`,
                );
                await page.waitForURL(
                  `**/site/${site.id}/view-availability/week?date=${week.week}`,
                );
                await page.waitForSelector('.nhsuk-loader', {
                  state: 'detached',
                });
              });

              test(`View week page data is arranged in the day cards as expected`, async () => {
                await weekViewAvailabilityPage.verifyViewNextAndPreviousWeeksButtonsDisplayed(
                  week.previousWeek,
                  week.nextWeek,
                );
                await weekViewAvailabilityPage.verifyAllDayCardInformationDisplayedCorrectly(
                  week.dayOverviews,
                );
              });
            });
          });

          sessionTestCases.forEach(daySession => {
            test.describe(`Session tests for day: '${daySession.dayCardHeader}'`, () => {
              test.beforeEach(async () => {
                //start test by navigating to the week view that contains this session
                await page.goto(
                  `manage-your-appointments/site/${site.id}/view-availability/week?date=${daySession.week}`,
                );
                await page.waitForURL(
                  `**/site/${site.id}/view-availability/week?date=${daySession.week}`,
                );
                await page.waitForSelector('.nhsuk-loader', {
                  state: 'detached',
                });
              });

              test('Change session has the correct information on the edit session decision page', async () => {
                const changeButton = page
                  .getByRole('heading', {
                    name: daySession.dayCardHeader,
                  })
                  .locator('..')
                  .getByRole('link', {
                    name: 'Change',
                  });

                await changeButton.click();

                await page.waitForURL(
                  `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
                );

                expect(changeAvailabilityPage.changeHeader).toHaveText(
                  `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
                );

                //table headers!
                await expect(
                  changeAvailabilityPage.page.getByRole('columnheader', {
                    name: 'Time',
                  }),
                ).toBeVisible();

                await expect(
                  changeAvailabilityPage.page.getByRole('columnheader', {
                    name: 'Services',
                  }),
                ).toBeVisible();

                await expect(
                  changeAvailabilityPage.page.getByRole('columnheader', {
                    name: 'Booked',
                    exact: true,
                  }),
                ).toBeVisible();

                await expect(
                  changeAvailabilityPage.page.getByRole('columnheader', {
                    name: 'Unbooked',
                    exact: true,
                  }),
                ).toBeVisible();

                //no action header
                await expect(
                  changeAvailabilityPage.page.getByRole('columnheader', {
                    name: 'Action',
                  }),
                ).not.toBeVisible();

                const timeCell = changeAvailabilityPage.page.getByRole('cell', {
                  name: daySession.timeRange,
                });
                const serviceCell = changeAvailabilityPage.page.getByRole(
                  'cell',
                  {
                    name: daySession.service,
                  },
                );
                const bookedCell = changeAvailabilityPage.page.getByRole(
                  'cell',
                  {
                    name: `${daySession.booked} booked`,
                  },
                );
                const unbookedCell = changeAvailabilityPage.page.getByRole(
                  'cell',
                  {
                    name: `${daySession.unbooked} unbooked`,
                  },
                );

                await expect(timeCell).toBeVisible();
                await expect(serviceCell).toBeVisible();
                await expect(bookedCell).toBeVisible();
                await expect(unbookedCell).toBeVisible();
              });

              test('Change session has the correct information on the edit session page', async () => {
                const changeButton = page
                  .getByRole('heading', {
                    name: daySession.dayCardHeader,
                  })
                  .locator('..')
                  .getByRole('link', {
                    name: 'Change',
                  });

                await changeButton.click();

                await page.waitForURL(
                  `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
                );

                expect(changeAvailabilityPage.changeHeader).toHaveText(
                  `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
                );

                await changeAvailabilityPage.selectChangeType(
                  'ChangeLengthCapacity',
                );
                await changeAvailabilityPage.saveChanges();

                await page.waitForURL(
                  `**/site/${site.id}/availability/edit?session**`,
                );

                await expect(addSessionPage.addSessionHeader).toHaveText(
                  `Edit sessionEdit time and capacity for ${daySession.changeSessionHeader}`,
                );

                await expect(
                  addSessionPage.page.getByText(
                    `${daySession.booked} booked appointments in this session.`,
                  ),
                ).toBeVisible();
                await expect(
                  addSessionPage.page.getByText(
                    `${daySession.unbooked} unbooked appointments in this session.`,
                  ),
                ).toBeVisible();

                await expect(addSessionPage.startTimeHour).toHaveValue(
                  daySession.startHour,
                );
                await expect(addSessionPage.startTimeMinute).toHaveValue(
                  daySession.startMins,
                );
                await expect(addSessionPage.endTimeHour).toHaveValue(
                  daySession.endHour,
                );
                await expect(addSessionPage.endTimeMinute).toHaveValue(
                  daySession.endMins,
                );
              });

              test('Change session has the correct information on the cancel session page', async () => {
                const changeButton = page
                  .getByRole('heading', {
                    name: daySession.dayCardHeader,
                  })
                  .locator('..')
                  .getByRole('link', {
                    name: 'Change',
                  });

                await changeButton.click();

                await page.waitForURL(
                  `**/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
                );

                expect(changeAvailabilityPage.changeHeader).toHaveText(
                  `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
                );

                await changeAvailabilityPage.selectChangeType('CancelSession');
                await changeAvailabilityPage.saveChanges();

                await page.waitForURL(
                  `**/site/${site.id}/availability/cancel?session**`,
                );

                await expect(
                  cancelSessionDetailsPage.cancelSessionHeader,
                ).toHaveText(
                  'Cancel sessionAre you sure you want to cancel this session?',
                );

                //single table
                await expect(
                  cancelSessionDetailsPage.page.locator('table'),
                ).toHaveCount(1);

                //table headers!
                await expect(
                  cancelSessionDetailsPage.page.getByRole('columnheader', {
                    name: 'Time',
                  }),
                ).toBeVisible();

                await expect(
                  cancelSessionDetailsPage.page.getByRole('columnheader', {
                    name: 'Services',
                  }),
                ).toBeVisible();

                await expect(
                  cancelSessionDetailsPage.page.getByRole('columnheader', {
                    name: 'Booked',
                    exact: true,
                  }),
                ).toBeVisible();

                await expect(
                  cancelSessionDetailsPage.page.getByRole('columnheader', {
                    name: 'Unbooked',
                    exact: true,
                  }),
                ).toBeVisible();

                //no actions
                await expect(
                  cancelSessionDetailsPage.page.getByRole('columnheader', {
                    name: 'Action',
                  }),
                ).not.toBeVisible();

                const timeCell = cancelSessionDetailsPage.page.getByRole(
                  'cell',
                  {
                    name: daySession.timeRange,
                  },
                );
                const serviceCell = cancelSessionDetailsPage.page.getByRole(
                  'cell',
                  {
                    name: daySession.service,
                  },
                );
                const bookedCell = cancelSessionDetailsPage.page.getByRole(
                  'cell',
                  {
                    name: `${daySession.booked} booked`,
                  },
                );
                const unbookedCell = cancelSessionDetailsPage.page.getByRole(
                  'cell',
                  {
                    name: `${daySession.unbooked} unbooked`,
                  },
                );

                await expect(timeCell).toBeVisible();
                await expect(serviceCell).toBeVisible();
                await expect(bookedCell).toBeVisible();
                await expect(unbookedCell).toBeVisible();
              });

              test('View daily appointments has the correct information, and on the cancel appointment page', async () => {
                const viewDailyAppointmentsButton = page
                  .getByRole('heading', {
                    name: daySession.dayCardHeader,
                  })
                  .locator('..')
                  .getByRole('link', {
                    name: 'View daily appointments',
                  });

                await viewDailyAppointmentsButton.click();

                await page.waitForURL(
                  `**/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
                );

                await dailyAppointmentDetailsPage.verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
                  daySession.viewDailyAppointments,
                );

                const allTableRows =
                  await dailyAppointmentDetailsPage.appointmentsTable
                    .getByRole('row')
                    .all();

                //dive into the cancel details page and verify information is correct
                for (
                  let index = 0;
                  index < daySession.cancelDailyAppointments.length;
                  index++
                ) {
                  const expectedAppointment =
                    daySession.cancelDailyAppointments[index];

                  //start at 1 to ignore header row
                  const tableRow = allTableRows[index + 1];

                  const cancelLink = tableRow.getByRole('link', {
                    name: 'Cancel',
                  });

                  await expect(cancelLink).toBeEnabled();

                  await cancelLink.click();

                  await page.waitForURL(
                    `**/site/${site.id}/appointment/**/cancel`,
                  );

                  await cancelAppointmentDetailsPage.verifyAppointmentDetailsDisplayed(
                    expectedAppointment,
                  );

                  //need to go back after this check
                  await cancelAppointmentDetailsPage.backButton.click();

                  await page.waitForURL(
                    `**/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
                  );
                }
              });
            });
          });
        });
      });
    });
  });
});

