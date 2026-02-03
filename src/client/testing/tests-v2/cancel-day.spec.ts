import { test, expect } from '../fixtures-v2';
import { daysFromToday, weekHeaderText } from '../utils/date-utility';

test.describe.configure({ mode: 'serial' });

const dayIncrement = 29;
const date = daysFromToday(dayIncrement);
const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
const requiredWeekRange = weekHeaderText(date);

// Use a shared variable for siteId since we are in serial mode
let siteId: string;

test.beforeEach(async ({ 
  page, 
  setUpSingleSite, 
  monthViewAvailabilityPage, 
  weekViewAvailabilityPage, 
  addSessionPage, 
  addServicesPage, 
  checkSessionDetailsPage 
}) => {
  // 1. Setup site/user and feature flag via v2 fixture
  const setup = await setUpSingleSite({
    features: [{ name: 'CancelDay', enabled: true }],
  });

  // 2. Extract siteId from the URL or the setup object
  // Based on your fixtures-v2.ts, the login flow ends on site selection
  const url = page.url();
  siteId = url.split('/site/')[1].split('/')[0];

  // 3. Navigate to the date
  await page.goto(`/manage-your-appointments/site/${siteId}/view-availability?date=${date}`);
  
  // 4. Navigate through Month View to Week View (matching your old working test)
  await monthViewAvailabilityPage.verifyViewMonthDisplayed(requiredWeekRange);
  await monthViewAvailabilityPage.openWeekViewHavingDate(requiredWeekRange);

  await page.waitForURL('**/site/**/view-availability/week?date=**');
  await page.waitForSelector('.nhsuk-loader', { state: 'detached' });
  
  // 5. Create the session that we will eventually cancel
  await weekViewAvailabilityPage.addAvailability(requiredDate);
  await addSessionPage.addSession('9', '00', '10', '00', '1', '5');
  await addServicesPage.addService('RSV Adult');
  await checkSessionDetailsPage.saveSession();

  await page.waitForURL('**/site/**/view-availability/week?date=**');
});

test('Cancel a day', async ({ 
  page, 
  weekViewAvailabilityPage, 
  cancelDayForm, 
  confirmedCancellationPage 
}) => {
  await weekViewAvailabilityPage.verifyCancelDayLinkDisplayed();
  await weekViewAvailabilityPage.cancelDayLink.click();
  
  await page.waitForURL(`**/site/${siteId}/cancel-day?date=${date}`);
  await cancelDayForm.verifyHeadingDisplayed(date);

  await cancelDayForm.cancelDayRadio.click();
  await cancelDayForm.continueButton.click();
  await cancelDayForm.cancelDayButton.click();

  await page.waitForURL(`**/site/${siteId}/cancel-day/confirmed?date=${date}*`);
  await confirmedCancellationPage.verifyCorrectTitleDisplayed(date);
  await confirmedCancellationPage.verifyViewCancelledApptWithoutContactDetailsVisibility(false);
});

test('Selecting no does not cancel a day', async ({ 
  weekViewAvailabilityPage, 
  cancelDayForm, 
  page 
}) => {
  await weekViewAvailabilityPage.cancelDayLink.click();
  await page.waitForURL(`**/site/${siteId}/cancel-day?date=${date}`);
  
  await cancelDayForm.dontCancelDayRadio.click();
  await cancelDayForm.continueButton.click();

  await page.waitForURL('**/site/**/view-availability/week?date=**');
  await weekViewAvailabilityPage.verifyCancelDayLinkDisplayed();
});

test('Selecting go back does not cancel a day', async ({ 
  weekViewAvailabilityPage, 
  cancelDayForm, 
  page 
}) => {
  await weekViewAvailabilityPage.cancelDayLink.click();
  await page.waitForURL(`**/site/${siteId}/cancel-day?date=${date}`);

  await cancelDayForm.cancelDayRadio.click();
  await cancelDayForm.continueButton.click();
  await cancelDayForm.goBackLink.click();

  await page.waitForURL('**/site/**/view-availability/week?date=**');
  await weekViewAvailabilityPage.verifyCancelDayLinkDisplayed();
});