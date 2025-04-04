import { test } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import ViewMonthAvailabilityPage from '../../page-objects/view-availability-appointment-pages/view-month-availability-page';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let viewMonthAvailabilityPage: ViewMonthAvailabilityPage;
let site: Site;

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  viewMonthAvailabilityPage = new ViewMonthAvailabilityPage(page, [
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
      services: [{ serviceName: 'RSV (Adult)', bookedAppointments: 0 }],
      totalAppointments: 840,
      booked: 0,
      unbooked: 840,
    },
    {
      header: '27 October to 2 November',
      services: [],
      totalAppointments: 0,
      booked: 0,
      unbooked: 0,
    },
  ]);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();

  //go to a month page that has a daylight savings change
  await page.goto(
    `manage-your-appointments/site/${site.id}/view-availability?date=2025-10-20`,
  );
});

test('All the view month page data is arranged in the week cards as expected', async () => {
  await viewMonthAvailabilityPage.verifyViewNextMonthButtonDisplayed();
  await viewMonthAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly();
});
