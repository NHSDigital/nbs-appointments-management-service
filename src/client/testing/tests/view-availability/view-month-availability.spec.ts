import {
  MonthViewAvailabilityPage,
  OAuthLoginPage,
  RootPage,
} from '@testing-page-objects';
import { test } from '../../fixtures';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let viewMonthAvailabilityPage: MonthViewAvailabilityPage;
let site: Site;

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

      test.describe('View Month Availability', () => {
        test.beforeEach(async ({ page, getTestSite }) => {
          site = getTestSite(2);
          rootPage = new RootPage(page);
          oAuthPage = new OAuthLoginPage(page);
          viewMonthAvailabilityPage = new MonthViewAvailabilityPage(page);

          await rootPage.goto();
          await rootPage.pageContentLogInButton.click();
          await oAuthPage.signIn();
        });

        test('All the month page data is arranged in the week cards as expected - Oct 2025', async ({
          page,
        }) => {
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

          await viewMonthAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
            'September 2025',
            'November 2025',
          );
          await viewMonthAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
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
                  { serviceName: 'RSV (Adult)', bookedAppointments: 4 },
                ],
                totalAppointments: 840,
                booked: 4,
                unbooked: 836,
              },
              {
                header: '27 October to 2 November',
                services: [
                  { serviceName: 'RSV (Adult)', bookedAppointments: 2 },
                ],
                totalAppointments: 420,
                booked: 2,
                unbooked: 418,
              },
            ],
          );
        });

        test('All the month page data is arranged in the week cards as expected - March 2026', async ({
          page,
        }) => {
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

          await viewMonthAvailabilityPage.verifyViewNextAndPreviousMonthButtonsAreDisplayed(
            'February 2026',
            'April 2026',
          );
          await viewMonthAvailabilityPage.verifyAllWeekCardInformationDisplayedCorrectly(
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
                  { serviceName: 'RSV (Adult)', bookedAppointments: 4 },
                ],
                totalAppointments: 480,
                booked: 4,
                unbooked: 476,
              },
              {
                header: '30 March to 5 April',
                services: [
                  { serviceName: 'RSV (Adult)', bookedAppointments: 2 },
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
  },
);
