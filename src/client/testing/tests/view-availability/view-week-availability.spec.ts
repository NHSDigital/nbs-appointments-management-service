import { test } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import { Site } from '@types';
import ViewWeekAvailabilityPage from '../../page-objects/view-availability-appointment-pages/view-week-availability-page';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let viewWeekAvailabilityPage: ViewWeekAvailabilityPage;
let site: Site;

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  viewWeekAvailabilityPage = new ViewWeekAvailabilityPage(page, [
    {
      header: 'Monday 20 October',
      services: [],
      totalAppointments: 0,
      booked: 0,
      unbooked: 0,
    },
    {
      header: 'Tuesday 21 October',
      services: [],
      totalAppointments: 0,
      booked: 0,
      unbooked: 0,
    },
    {
      header: 'Wednesday 22 October',
      services: [],
      totalAppointments: 0,
      booked: 0,
      unbooked: 0,
    },
    {
      header: 'Thursday 23 October',
      services: [],
      totalAppointments: 0,
      booked: 0,
      unbooked: 0,
    },
    {
      header: 'Friday 24 October',
      services: [],
      totalAppointments: 0,
      booked: 0,
      unbooked: 0,
    },
    {
      header: 'Saturday 25 October',
      services: [
        {
          serviceName: 'RSV (Adult)',
          booked: 0,
          unbooked: 420,
          sessionTimeInterval: '10:00 - 17:00',
        },
      ],
      totalAppointments: 420,
      booked: 0,
      unbooked: 420,
    },
    {
      header: 'Sunday 26 October',
      services: [
        {
          serviceName: 'RSV (Adult)',
          booked: 0,
          unbooked: 420,
          sessionTimeInterval: '10:00 - 17:00',
        },
      ],
      totalAppointments: 420,
      booked: 0,
      unbooked: 420,
    },
  ]);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();

  //go to a week page that has a daylight savings change
  await page.goto(
    `manage-your-appointments/site/${site.id}/view-availability/week?date=2025-10-20`,
  );
});

test('All the view week page data is arranged in the day cards as expected', async () => {
  await viewWeekAvailabilityPage.verifyViewNextWeekButtonDisplayed();
  await viewWeekAvailabilityPage.verifyAllDayCardInformationDisplayedCorrectly();
});
