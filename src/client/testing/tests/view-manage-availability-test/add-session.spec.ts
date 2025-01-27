import { test } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import MonthViewAvailabilityPage from '../../page-objects/view-availability-appointment-pages/month-view-availability-page';
import {
  getDateInFuture,
  geRequiredtDateInFormat,
} from '../../utils/date-utility';
import dayjs from 'dayjs';

const { TEST_USERS } = env;
let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let monthViewAvailabilityPage: MonthViewAvailabilityPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  monthViewAvailabilityPage = new MonthViewAvailabilityPage(page);

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
  // const requiredDate=geRequiredtDateInFormat('Tommorow','DD MMMM');

  //await monthViewAvailabilityPage.openWeekViewHavingDate(requiredDate);
  await page.waitForTimeout(10000);
});
