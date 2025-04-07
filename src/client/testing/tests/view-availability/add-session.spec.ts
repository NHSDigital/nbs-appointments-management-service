import { test } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import MonthViewAvailabilityPage from '../../page-objects/view-availability-appointment-pages/month-view-availability-page';
import { daysFromToday, weekHeaderText } from '../../utils/date-utility';
import WeekViewAvailabilityPage from '../../page-objects/view-availability-appointment-pages/week-view-availability-page';
import AddSessionPage from '../../page-objects/view-availability-appointment-pages/add-session-page';
import AddServicesPage from '../../page-objects/view-availability-appointment-pages/add-services-page';
import CheckSessionDetailsPage from '../../page-objects/view-availability-appointment-pages/check-session-details-page';
import ChangeAvailabilityPage from '../../page-objects/view-availability-appointment-pages/change-availability-page';
import CancelSessionDetailsPage from '../../page-objects/view-availability-appointment-pages/cancel-session-details-page';
import DailyAppointmentDetailsPage from '../../page-objects/view-availability-appointment-pages/daily-appointment-details-page';
import dayjs from 'dayjs';

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
let cancelSessionDetailsPage: CancelSessionDetailsPage;
let dailyAppointmentDetailsPage: DailyAppointmentDetailsPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  monthViewAvailabilityPage = new MonthViewAvailabilityPage(page, []);
  weekViewAvailabilityPage = new WeekViewAvailabilityPage(page, []);
  addSessionPage = new AddSessionPage(page);
  addServicesPage = new AddServicesPage(page);
  checkSessionDetailsPage = new CheckSessionDetailsPage(page);
  changeAvailabilityPage = new ChangeAvailabilityPage(page);
  cancelSessionDetailsPage = new CancelSessionDetailsPage(page);
  dailyAppointmentDetailsPage = new DailyAppointmentDetailsPage(page);

  await rootPage.goto();
  await rootPage.cookieBanner.acceptCookiesButton.click();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();

  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await page.waitForURL('**/site/**/view-availability');
});

test('Verify user is able to add a session for future date', async () => {
  const requiredDate = daysFromToday(1, 'D MMMM');
  const requiredWeekRange = weekHeaderText(daysFromToday(1));
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.verifyAddSessionPageDisplayed();
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.verifyAddServicesPageDisplayed();
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.verifyCheckSessionDetailsPageDisplayed();
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
});

test('Verify add availability option displayed for future date', async () => {
  const requiredDate = daysFromToday(2, 'D MMMM');
  const requiredWeekRange = weekHeaderText(daysFromToday(2));
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
  await weekViewAvailabilityPage.verifyAddAvailabilityButtonDisplayed(
    requiredDate,
  );
});

test('Verify user is able to change availability', async () => {
  const requiredDate = daysFromToday(1, 'D MMMM');
  const requiredWeekRange = weekHeaderText(daysFromToday(1));
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openChangeAvailabilityPage(requiredDate);
  await changeAvailabilityPage.selectChangeType('ChangeLengthCapacity');
  await changeAvailabilityPage.saveChanges();
  await addSessionPage.updateSessionEndTime('9', '30');
  await changeAvailabilityPage.verifySessionUpdated();
});

test('Verify user is able to cancel session', async () => {
  const requiredDate = daysFromToday(5, 'D MMMM');
  const requiredWeekRange = weekHeaderText(daysFromToday(5));
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openChangeAvailabilityPage(requiredDate);
  await changeAvailabilityPage.selectChangeType('CancelSession');
  await changeAvailabilityPage.saveChanges();
  await cancelSessionDetailsPage.confirmSessionCancelation('Yes');
  const cancelDate = daysFromToday(5, 'DD MMMM');
  await cancelSessionDetailsPage.verifySessionCancelled(cancelDate);
});

test('Verify session not canceled if not confirmed', async () => {
  const requiredDate = daysFromToday(3, 'D MMMM');
  const requiredWeekRange = weekHeaderText(daysFromToday(3));
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openChangeAvailabilityPage(requiredDate);
  await changeAvailabilityPage.selectChangeType('CancelSession');
  await changeAvailabilityPage.saveChanges();
  await cancelSessionDetailsPage.confirmSessionCancelation('No');
  await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
  await dailyAppointmentDetailsPage.navigateToWeekView();
  await weekViewAvailabilityPage.verifySessionRecordDetail(
    requiredDate,
    '09:00 - 10:00',
    'RSV (Adult)',
  );
});

test('Verify view daily appointment link displayed', async () => {
  const requiredDate = daysFromToday(3, 'D MMMM');
  const requiredWeekRange = weekHeaderText(daysFromToday(3));
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openDailyAppoitmentPage(requiredDate);
  await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
});

test('Verify appointment not cancelled when not confirmed', async () => {
  const requiredDate = '2025-08-05';
  const formattedDate = dayjs(requiredDate).format('D MMMM');
  await monthViewAvailabilityPage.navigateToRequiredMonth(requiredDate);
  const requiredWeekRange = weekHeaderText(requiredDate);
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(formattedDate);
  await weekViewAvailabilityPage.openDailyAppoitmentPage(formattedDate);
  await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
  await dailyAppointmentDetailsPage.cancelAppointment('5932817282');
  await dailyAppointmentDetailsPage.confirmAppointmentCancelation('No');
  await dailyAppointmentDetailsPage.verifyAppointmentNotCancelled('5932817282');
});

test('Verify availibility with no bookings is cancelled and manual appointments folder is empty', async () => {
  const requiredDate = daysFromToday(1, 'D MMMM');
  const formattedDate = dayjs(requiredDate).format('DD MMMM');
  const requiredWeekRange = weekHeaderText(daysFromToday(1));
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed(requiredDate);
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openChangeAvailabilityPage(requiredDate);
  await changeAvailabilityPage.selectChangeType('CancelSession');
  await changeAvailabilityPage.saveChanges();
  await cancelSessionDetailsPage.confirmSessionCancelation('Yes');
  await cancelSessionDetailsPage.verifySessionCancelled(formattedDate);
  await cancelSessionDetailsPage.clickCancelAppointment();
  await dailyAppointmentDetailsPage.verifyManualAppointment();
});
