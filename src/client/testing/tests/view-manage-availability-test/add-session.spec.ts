import { test } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import MonthViewAvailabilityPage from '../../page-objects/view-availability-appointment-pages/month-view-availability-page';
import {
  geRequiredtDateInFormat,
  getWeekRange,
} from '../../utils/date-utility';
import WeekViewAvailabilityPage from '../../page-objects/view-availability-appointment-pages/week-view-availability-page';
import AddSessionPage from '../../page-objects/view-availability-appointment-pages/add-session-page';
import AddServicesPage from '../../page-objects/view-availability-appointment-pages/add-services-page';
import CheckSessionDetailsPage from '../../page-objects/view-availability-appointment-pages/check-session-details-page';
import ChangeAvailabilityPage from '../../page-objects/view-availability-appointment-pages/change-availability-page';
import CancelSessionDetailsPage from '../../page-objects/view-availability-appointment-pages/cancel-session-details-page';
import DailyAppointmentDetailsPage from '../../page-objects/view-availability-appointment-pages/daily-appointment-details-page';

const { TEST_USERS } = env;
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
  monthViewAvailabilityPage = new MonthViewAvailabilityPage(page);
  weekViewAvailabilityPage = new WeekViewAvailabilityPage(page);
  addSessionPage = new AddSessionPage(page);
  addServicesPage = new AddServicesPage(page);
  checkSessionDetailsPage = new CheckSessionDetailsPage(page);
  changeAvailabilityPage = new ChangeAvailabilityPage(page);
  cancelSessionDetailsPage = new CancelSessionDetailsPage(page);
  dailyAppointmentDetailsPage = new DailyAppointmentDetailsPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await page.waitForURL('**/site/**/view-availability');
});

test('Verify user is able to add a session for future date', async () => {
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat(1, 'D MMMM');
  const requiredWeekRange = getWeekRange(1);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
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
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat(10, 'D MMMM');
  const requiredWeekRange = getWeekRange(10);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
  await weekViewAvailabilityPage.verifyAddAvailabilityButtonDisplayed(
    requiredDate,
  );
});

test('Verify user is able to change availability', async () => {
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat(1, 'D MMMM');
  const requiredWeekRange = getWeekRange(1);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openChangeAvailabilityPage(requiredDate);
  await changeAvailabilityPage.selectChangeType('ShortenLenght');
  await changeAvailabilityPage.saveChanges();
  await addSessionPage.updateSessionEndTime('9', '30');
  await changeAvailabilityPage.verifySessionUpdated();
});

test('Verify user is able to cancel session', async () => {
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat(10, 'D MMMM');
  const requiredWeekRange = getWeekRange(10);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openChangeAvailabilityPage(requiredDate);
  await changeAvailabilityPage.selectChangeType('CancelSession');
  await changeAvailabilityPage.saveChanges();
  await cancelSessionDetailsPage.confirmSessionCancelation('Yes');
  await cancelSessionDetailsPage.verifySessionCancelled(requiredDate);
});

test('Verify session not canceled if not confirmed', async () => {
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat(10, 'D MMMM');
  const requiredWeekRange = getWeekRange(10);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
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
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat(10, 'D MMMM');
  const requiredWeekRange = getWeekRange(10);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openDailyAppoitmentPage(requiredDate);
  await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
});

test('Verify user is able to cancel an appoitment', async () => {
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat(1, 'D MMMM');
  const requiredWeekRange = getWeekRange(1);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
  await weekViewAvailabilityPage.openDailyAppoitmentPage(requiredDate);
  await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
  await dailyAppointmentDetailsPage.cancelAppointment('9:00');
  await dailyAppointmentDetailsPage.verifyDailyAppointmentDetailsPageDisplayed();
});
