import { test } from '../../fixtures';
import {
  AddServicesPage,
  AddSessionPage,
  CancelSessionDetailsPage,
  ChangeAvailabilityPage,
  CheckSessionDetailsPage,
  DailyAppointmentDetailsPage,
  EditAvailabilityConfirmedPage,
  MonthViewAvailabilityPage,
  OAuthLoginPage,
  RootPage,
  SitePage,
  SiteSelectionPage,
  WeekViewAvailabilityPage,
} from '@testing-page-objects';
import { daysFromToday, weekHeaderText } from '../../utils/date-utility';
import { parseToUkDatetime } from '@services/timeService';

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

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    //nhsd-jira.digital.nhs.uk/browse/APPT-867
    test.describe.fixme(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      test.describe('Add Session', () => {
        test.beforeEach(async ({ page }) => {
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

          await siteSelectionPage.selectSite('Church Lane Pharmacy');
          await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
          await page.waitForURL('**/site/**/view-availability');
        });

        test('Verify user is able to add a session for future date', async ({
          page,
        }) => {
          const requiredDate = daysFromToday(1, 'D MMMM');
          const requiredWeekRange = weekHeaderText(daysFromToday(1));

          await monthViewAvailabilityPage.verifyViewMonthDisplayed(
            requiredWeekRange,
          );
          await monthViewAvailabilityPage.openWeekViewHavingDate(
            requiredWeekRange,
          );

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
          await weekViewAvailabilityPage.addAvailability(requiredDate);

          await page.waitForURL(
            '**/site/**/create-availability/wizard?date=**',
          );

          await addSessionPage.verifyAddSessionPageDisplayed();
          await addSessionPage.addSession('9', '00', '10', '00', '1', '5');

          await addServicesPage.verifyAddServicesPageDisplayed();
          await addServicesPage.addService('RSV (Adult)');

          await checkSessionDetailsPage.verifyCheckSessionDetailsPageDisplayed();
          await checkSessionDetailsPage.saveSession();

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifySessionAdded();
        });

        test('Verify add availability option displayed for future date', async ({
          page,
        }) => {
          const requiredDate = daysFromToday(2, 'D MMMM');
          const requiredWeekRange = weekHeaderText(daysFromToday(2));
          await monthViewAvailabilityPage.verifyViewMonthDisplayed(
            requiredWeekRange,
          );
          await monthViewAvailabilityPage.openWeekViewHavingDate(
            requiredWeekRange,
          );

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
          await weekViewAvailabilityPage.verifyAddAvailabilityButtonDisplayed(
            requiredDate,
          );
        });

        test('Verify user is able to change availability', async ({ page }) => {
          const requiredDate = daysFromToday(1, 'D MMMM');
          const requiredWeekRange = weekHeaderText(daysFromToday(1));

          await monthViewAvailabilityPage.verifyViewMonthDisplayed(
            requiredWeekRange,
          );
          await monthViewAvailabilityPage.openWeekViewHavingDate(
            requiredWeekRange,
          );

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
          await weekViewAvailabilityPage.addAvailability(requiredDate);

          await page.waitForURL(
            '**/site/**/create-availability/wizard?date=**',
          );

          await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
          await addServicesPage.addService('RSV (Adult)');
          await checkSessionDetailsPage.saveSession();

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifySessionAdded();
          await weekViewAvailabilityPage.openChangeAvailabilityPage(
            requiredDate,
          );

          await page.waitForURL(
            '**/site/**/view-availability/week/edit-session?date=**',
          );

          await changeAvailabilityPage.selectChangeType('ChangeLengthCapacity');
          await changeAvailabilityPage.saveChanges();

          await page.waitForURL('**/site/**/availability/edit?session=**');

          await addSessionPage.updateSessionEndTime('9', '30');

          await page.waitForURL(
            '**/site/**/availability/edit/confirmed?updatedSession=**',
          );
          await editAvailabilityConfirmedPage.verifySessionUpdated();
        });

        test('Verify user is able to cancel session', async ({ page }) => {
          const requiredDate = daysFromToday(5, 'D MMMM');
          const requiredWeekRange = weekHeaderText(daysFromToday(5));

          await monthViewAvailabilityPage.verifyViewMonthDisplayed(
            requiredWeekRange,
          );
          await monthViewAvailabilityPage.openWeekViewHavingDate(
            requiredWeekRange,
          );

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
          await weekViewAvailabilityPage.addAvailability(requiredDate);

          await page.waitForURL(
            '**/site/**/create-availability/wizard?date=**',
          );

          await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
          await addServicesPage.addService('RSV (Adult)');
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

          const cancelDate = daysFromToday(5, 'DD MMMM');
          await cancelSessionDetailsPage.verifySessionCancelled(cancelDate);
        });

        test('Verify session not canceled if not confirmed', async ({
          page,
        }) => {
          const requiredDate = daysFromToday(3, 'D MMMM');
          const requiredWeekRange = weekHeaderText(daysFromToday(3));

          await monthViewAvailabilityPage.verifyViewMonthDisplayed(
            requiredWeekRange,
          );
          await monthViewAvailabilityPage.openWeekViewHavingDate(
            requiredWeekRange,
          );

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
          await weekViewAvailabilityPage.addAvailability(requiredDate);

          await page.waitForURL(
            '**/site/**/create-availability/wizard?date=**',
          );

          await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
          await addServicesPage.addService('RSV (Adult)');
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
            'RSV (Adult)',
          );
        });

        test('Verify view daily appointment link displayed', async ({
          page,
        }) => {
          const requiredDate = daysFromToday(3, 'D MMMM');
          const requiredWeekRange = weekHeaderText(daysFromToday(3));

          await monthViewAvailabilityPage.verifyViewMonthDisplayed(
            requiredWeekRange,
          );
          await monthViewAvailabilityPage.openWeekViewHavingDate(
            requiredWeekRange,
          );

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
          await weekViewAvailabilityPage.addAvailability(requiredDate);

          await page.waitForURL(
            '**/site/**/create-availability/wizard?date=**',
          );

          await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
          await addServicesPage.addService('RSV (Adult)');
          await checkSessionDetailsPage.saveSession();

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifySessionAdded();
          await weekViewAvailabilityPage.openDailyAppointmentPage(requiredDate);
          await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
        });

        test('Verify appointment not cancelled when not confirmed', async ({
          page,
        }) => {
          const requiredDate = '2025-08-05';
          const formattedDate =
            parseToUkDatetime(requiredDate).format('D MMMM');
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

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(formattedDate);
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
          const requiredDate = daysFromToday(1);
          const formattedDate1 =
            parseToUkDatetime(requiredDate).format('DD MMMM');
          const formattedDate2 =
            parseToUkDatetime(requiredDate).format('D MMMM');
          const requiredWeekRange = weekHeaderText(daysFromToday(1));

          await monthViewAvailabilityPage.verifyViewMonthDisplayed(
            requiredWeekRange,
          );
          await monthViewAvailabilityPage.openWeekViewHavingDate(
            requiredWeekRange,
          );

          await page.waitForURL('**/site/**/view-availability/week?date=**');

          await weekViewAvailabilityPage.verifyWeekViewDisplayed(
            formattedDate2,
          );
          await weekViewAvailabilityPage.addAvailability(formattedDate2);

          await page.waitForURL(
            '**/site/**/create-availability/wizard?date=**',
          );

          await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
          await addServicesPage.addService('RSV (Adult)');
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

          await cancelSessionDetailsPage.verifySessionCancelled(formattedDate1);
          await cancelSessionDetailsPage.clickCancelAppointment();

          await page.waitForURL(
            '**/site/**/view-availability/daily-appointments?date=**',
          );

          await dailyAppointmentDetailsPage.verifyManualAppointment();
        });
      });
    });
  },
);
