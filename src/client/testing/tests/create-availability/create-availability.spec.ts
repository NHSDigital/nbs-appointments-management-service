import { test, expect } from '../../fixtures';

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(
      `Test in timezone: '${timezone}'`,
      { tag: [`@timezone-test-${timezone}`] },
      () => {
        test.use({ timezoneId: timezone });

        test.describe('Create Availability', () => {
          test(
            'A users views the availability created to date',
            { tag: '@affects:site2' },
            async ({ signInToSite }) => {
              await signInToSite(1, 2)
                .then(sitePage => sitePage.clickCreateAvailabilityCard())
                .then(async createAvailabilityPage => {
                  await expect(createAvailabilityPage.title).toBeVisible();
                  await expect(
                    createAvailabilityPage.availabilityCreatedTable,
                  ).toBeVisible();

                  await expect(
                    createAvailabilityPage.availabilityCreatedTable.getByRole(
                      'row',
                      {
                        name: '1 Jul 2025 Tue RSV (Adult) Single Date',
                      },
                    ),
                  ).toBeVisible();
                  await expect(
                    createAvailabilityPage.availabilityCreatedTable.getByRole(
                      'row',
                      {
                        name: '1 Jul 2025 - 5 Aug 2025 Mon, Wed, Fri, Sun RSV (Adult) Weekly repeating',
                      },
                    ),
                  ).toBeVisible();
                });
            },
          );
        });
      },
    );
  },
);
