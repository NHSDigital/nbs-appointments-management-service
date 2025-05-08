import { LoginPage, MonthViewPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import {
  expectEmptyWeek,
  expectWeekSummary,
} from './view-month-availability.methods';

let put: MonthViewPage;

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(
      `Test in timezone: '${timezone}'`,
      { tag: [`@timezone-test-${timezone}`] },
      () => {
        test.use({ timezoneId: timezone });

        test.describe('View Month Availability', () => {
          test.beforeEach(async ({ page, getTestSite }) => {
            put = await new LoginPage(page)
              .logInWithNhsMail()
              .then(oAuthPage => oAuthPage.signIn())
              .then(siteSelectionPage =>
                siteSelectionPage.selectSite(getTestSite()),
              )
              .then(sitePage => sitePage.clickViewAvailabilityCard());
          });

          test('All the month page data is arranged in the week cards as expected - Oct 2025', async () => {
            // Go to a specific month page that has a daylight savings change
            put = await put.goToSpecificDate('2025-10-01');
            await expect(put.previousMonthButton).toHaveText('September 2025');
            await expect(put.previousMonthButton).toHaveText('November 2025');

            await expectEmptyWeek(put, '29 September to 5 October');
            await expectEmptyWeek(put, '6 October to 12 October');
            await expectEmptyWeek(put, '13 October to 19 October');

            await expectWeekSummary(put, {
              weekTitle: '20 October to 26 October',
              services: [{ serviceName: 'RSV (Adult)', bookedAppointments: 4 }],
              totalAppointments: 840,
              booked: 4,
              unbooked: 836,
            });
            await expectWeekSummary(put, {
              weekTitle: '27 October to 2 November',
              services: [{ serviceName: 'RSV (Adult)', bookedAppointments: 2 }],
              totalAppointments: 420,
              booked: 2,
              unbooked: 418,
            });
          });

          test('All the month page data is arranged in the week cards as expected - March 2026', async () => {
            // Go to a specific month page that has a daylight savings change
            put = await put.goToSpecificDate('2026-03-01');
            await expect(put.previousMonthButton).toHaveText('February 2026');
            await expect(put.previousMonthButton).toHaveText('April 2026');

            await expectEmptyWeek(put, '23 February to 1 March');
            await expectEmptyWeek(put, '2 March to 8 March');
            await expectEmptyWeek(put, '9 March to 15 March');
            await expectEmptyWeek(put, '16 March to 22 March');
            await expectWeekSummary(put, {
              weekTitle: '23 March to 29 March',
              services: [{ serviceName: 'RSV (Adult)', bookedAppointments: 4 }],
              totalAppointments: 480,
              booked: 4,
              unbooked: 476,
            });
            await expectWeekSummary(put, {
              weekTitle: '30 March to 5 April',
              services: [{ serviceName: 'RSV (Adult)', bookedAppointments: 2 }],
              totalAppointments: 240,
              booked: 2,
              unbooked: 238,
            });
          });
        });
      },
    );
  },
);
