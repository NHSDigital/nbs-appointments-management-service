import {
  AddServicesPage,
  AddSessionPage,
  CheckSessionDetailsPage,
  MonthViewAvailabilityPage,
  OAuthLoginPage,
  RootPage,
  WeekViewAvailabilityPage,
} from '@testing-page-objects';
import CancelDayForm from '../page-objects/cancel-day-pages/cancel-day-form';
import ConfirmedCancellationPage from '../page-objects/cancel-day-pages/confirm-cancellation';
import { Site } from '@types';
import { test, overrideFeatureFlag } from '../fixtures';
import { daysFromToday, weekHeaderText } from '../utils/date-utility';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let weekViewAvailabilityPage: WeekViewAvailabilityPage;
let cancelDayForm: CancelDayForm;
let confirmedCancellationPage: ConfirmedCancellationPage;
let addSessionPage: AddSessionPage;
let monthViewAvailabilityPage: MonthViewAvailabilityPage;
let addServicesPage: AddServicesPage;
let checkSessionDetailsPage: CheckSessionDetailsPage;

let site: Site;

test.describe.configure({ mode: 'serial' });

const dayIncrement = 29;
const date = daysFromToday(dayIncrement);
const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
const requiredWeekRange = weekHeaderText(date);

test.beforeAll(async () => {
  await overrideFeatureFlag('CancelDay', true);
});

test.afterAll(async () => {
  await overrideFeatureFlag('CancelDay', false);
});

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(1);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  weekViewAvailabilityPage = new WeekViewAvailabilityPage(page);
  cancelDayForm = new CancelDayForm(page);
  confirmedCancellationPage = new ConfirmedCancellationPage(page);
  addSessionPage = new AddSessionPage(page);
  monthViewAvailabilityPage = new MonthViewAvailabilityPage(page);
  addServicesPage = new AddServicesPage(page);
  checkSessionDetailsPage = new CheckSessionDetailsPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();

  await page.goto(
    `/manage-your-appointments/site/${site.id}/view-availability?date=${date}`,
  );
  await page.waitForURL(
    `/manage-your-appointments/site/${site.id}/view-availability?date=${date}`,
  );

  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);

  await page.waitForURL('**/site/**/view-availability/week?date=**');
  await page.waitForSelector('.nhsuk-loader', {
    state: 'detached',
  });
  await weekViewAvailabilityPage.addAvailability(requiredDate);

  await page.waitForURL('**/site/**/create-availability/wizard?date=**');

  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV Adult');
  await checkSessionDetailsPage.saveSession();

  await page.waitForURL('**/site/**/view-availability/week?date=**');
});

test('Cancel a day', async ({ page }) => {
  await weekViewAvailabilityPage.verifyCancelDayLinkDisplayed();

  await weekViewAvailabilityPage.cancelDayLink.click();
  await page.waitForURL(`**/site/${site.id}/cancel-day?date=${date}`);
  await cancelDayForm.verifyHeadingDisplayed(date);

  await cancelDayForm.cancelDayRadio.click();
  await cancelDayForm.continueButton.click();
  await cancelDayForm.cancelDayButton.click();

  await page.waitForURL(
    `**/site/${site.id}/cancel-day/confirmed?date=${date}&cancelledBookingCount=0&bookingsWithoutContactDetails=0`,
  );
  await confirmedCancellationPage.verifyCorrectTitleDisplayed(date);
  await confirmedCancellationPage.verifyViewCancelledApptWithoutContactDetailsVisibility(
    false,
  );
});

test('Selecting no does not cancel a day', async ({ page }) => {
  await weekViewAvailabilityPage.cancelDayLink.click();
  await page.waitForURL(`**/site/${site.id}/cancel-day?date=${date}`);
  await cancelDayForm.verifyHeadingDisplayed(date);

  await cancelDayForm.dontCancelDayRadio.click();
  await cancelDayForm.continueButton.click();

  await page.waitForURL('**/site/**/view-availability/week?date=**');

  await weekViewAvailabilityPage.verifyCancelDayLinkDisplayed();
});

test('Selecting go back does not cancel a day', async ({ page }) => {
  await weekViewAvailabilityPage.cancelDayLink.click();
  await page.waitForURL(`**/site/${site.id}/cancel-day?date=${date}`);
  await cancelDayForm.verifyHeadingDisplayed(date);

  await cancelDayForm.cancelDayRadio.click();
  await cancelDayForm.continueButton.click();
  await cancelDayForm.goBackLink.click();

  await page.waitForURL('**/site/**/view-availability/week?date=**');

  await weekViewAvailabilityPage.verifyCancelDayLinkDisplayed();
});
