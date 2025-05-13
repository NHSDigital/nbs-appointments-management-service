import { test, expect } from '../../fixtures';
import {
  expectEmptyWeek,
  expectWeekSummary,
} from './view-month-availability.methods';

// let put: MonthViewPage;

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(
      `Test in timezone: '${timezone}'`,
      { tag: [`@timezone-test-${timezone}`] },
      () => {
        test.use({ timezoneId: timezone });

        test.describe('View Month Availability', () => {
          test('All the month page data is arranged in the week cards as expected - Oct 2025', async ({
            signInToSite,
          }) => {
            await signInToSite()
              .then(sitePage => sitePage.clickViewAvailabilityCard())
              .then(monthViewPage =>
                // Go to a specific month page that has a daylight savings change
                monthViewPage.goToSpecificDate('2025-10-01'),
              )
              .then(async monthViewPage => {
                await expect(monthViewPage.previousMonthButton).toHaveText(
                  'September 2025',
                );
                await expect(monthViewPage.previousMonthButton).toHaveText(
                  'November 2025',
                );

                await expectEmptyWeek(
                  monthViewPage,
                  '29 September to 5 October',
                );
                await expectEmptyWeek(monthViewPage, '6 October to 12 October');
                await expectEmptyWeek(
                  monthViewPage,
                  '13 October to 19 October',
                );

                await expectWeekSummary(monthViewPage, {
                  weekTitle: '20 October to 26 October',
                  services: [
                    { serviceName: 'RSV (Adult)', bookedAppointments: 4 },
                  ],
                  totalAppointments: 840,
                  booked: 4,
                  unbooked: 836,
                });
                await expectWeekSummary(monthViewPage, {
                  weekTitle: '27 October to 2 November',
                  services: [
                    { serviceName: 'RSV (Adult)', bookedAppointments: 2 },
                  ],
                  totalAppointments: 420,
                  booked: 2,
                  unbooked: 418,
                });
              });
          });

          test('All the month page data is arranged in the week cards as expected - March 2026', async ({
            signInToSite,
          }) => {
            await signInToSite()
              .then(sitePage => sitePage.clickViewAvailabilityCard())
              .then(monthViewPage =>
                // Go to a specific month page that has a daylight savings change
                monthViewPage.goToSpecificDate('2026-03-01'),
              )
              .then(async monthViewPage => {
                await expect(monthViewPage.previousMonthButton).toHaveText(
                  'February 2026',
                );
                await expect(monthViewPage.previousMonthButton).toHaveText(
                  'April 2026',
                );

                await expectEmptyWeek(monthViewPage, '23 February to 1 March');
                await expectEmptyWeek(monthViewPage, '2 March to 8 March');
                await expectEmptyWeek(monthViewPage, '9 March to 15 March');
                await expectEmptyWeek(monthViewPage, '16 March to 22 March');
                await expectWeekSummary(monthViewPage, {
                  weekTitle: '23 March to 29 March',
                  services: [
                    { serviceName: 'RSV (Adult)', bookedAppointments: 4 },
                  ],
                  totalAppointments: 480,
                  booked: 4,
                  unbooked: 476,
                });
                await expectWeekSummary(monthViewPage, {
                  weekTitle: '30 March to 5 April',
                  services: [
                    { serviceName: 'RSV (Adult)', bookedAppointments: 2 },
                  ],
                  totalAppointments: 240,
                  booked: 2,
                  unbooked: 238,
                });
              });
          });
        });
      },
    );
  },
);
