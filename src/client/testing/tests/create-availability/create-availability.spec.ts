import { CreateAvailabilityPage, LoginPage } from '@testing-page-objects';
import { test, expect } from '../../fixtures';

let put: CreateAvailabilityPage;

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(
      `Test in timezone: '${timezone}'`,
      { tag: [`@timezone-test-${timezone}`] },
      () => {
        test.use({ timezoneId: timezone });

        test.describe('Create Availability', () => {
          test.beforeEach(async ({ page, getTestSite }) => {
            put = await new LoginPage(page)
              .logInWithNhsMail()
              .then(oAuthPage => oAuthPage.signIn())
              .then(siteSelectionPage =>
                siteSelectionPage.selectSite(getTestSite(2)),
              )
              .then(sitePage => sitePage.clickCreateAvailabilityCard());
          });

          test(
            'A users views the availability created to date',
            { tag: '@affects:site2' },
            async () => {
              await expect(put.title).toBeVisible();
              await expect(put.availabilityCreatedTable).toBeVisible();

              await expect(
                put.availabilityCreatedTable.getByRole('row', {
                  name: '1 Jul 2025 Tue RSV (Adult) Single Date',
                }),
              ).toBeVisible();
              await expect(
                put.availabilityCreatedTable.getByRole('row', {
                  name: '1 Jul 2025 - 5 Aug 2025 Mon, Wed, Fri, Sun RSV (Adult) Weekly repeating',
                }),
              ).toBeVisible();
            },
          );
        });
      },
    );
  },
);
