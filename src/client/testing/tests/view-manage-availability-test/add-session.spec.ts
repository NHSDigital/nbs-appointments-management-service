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

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.viewAvailabilityAndManageAppointmentsCard.click();
  await page.waitForURL('**/site/**/view-availability');
});

test('Verify user is able to add a session for future date', async ({
  page,
}) => {
  await monthViewAvailabilityPage.verifyViewMonthDisplayed();
  const requiredDate = geRequiredtDateInFormat('Tommorow', 'DD MMMM');
  const requiredWeekRange = getWeekRange();
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);
  await weekViewAvailabilityPage.verifyWeekViewDisplayed();
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.verifyAddSessionPageDisplayed();
  await addSessionPage.addSession('9', '10', '1', '5');
  await addServicesPage.verifyAddServicesPageDisplayed();
  await addServicesPage.addService('RSV (Adult)');
  await checkSessionDetailsPage.verifyCheckSessionDetailsPageDisplayed();
  await checkSessionDetailsPage.saveSession();
  await weekViewAvailabilityPage.verifySessionAdded();
});
