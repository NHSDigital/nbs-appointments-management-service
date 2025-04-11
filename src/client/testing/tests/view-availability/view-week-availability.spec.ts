import {
  AddSessionPage,
  CancelSessionDetailsPage,
  ChangeAvailabilityPage,
  OAuthLoginPage,
  RootPage,
  WeekViewAvailabilityPage,
} from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let viewWeekAvailabilityPage: WeekViewAvailabilityPage;
let changeAvailabilityPage: ChangeAvailabilityPage;
let addSessionPage: AddSessionPage;
let cancelSessionPage: CancelSessionDetailsPage;
let site: Site;

test.describe.skip(
  'Daylight Savings Tests for the week view - Oct 20-27th 2025',
  () => {
    test.beforeEach(async ({ page, getTestSite }) => {
      site = getTestSite(2);
      rootPage = new RootPage(page);
      oAuthPage = new OAuthLoginPage(page);
      viewWeekAvailabilityPage = new WeekViewAvailabilityPage(page);
      changeAvailabilityPage = new ChangeAvailabilityPage(page);
      addSessionPage = new AddSessionPage(page);
      cancelSessionPage = new CancelSessionDetailsPage(page);

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
      await viewWeekAvailabilityPage.verifyAllDayCardInformationDisplayedCorrectly(
        [
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
                booked: 2,
                unbooked: 418,
                sessionTimeInterval: '10:00 - 17:00',
              },
            ],
            totalAppointments: 420,
            booked: 2,
            unbooked: 418,
          },
          {
            header: 'Sunday 26 October',
            services: [
              {
                serviceName: 'RSV (Adult)',
                booked: 2,
                unbooked: 418,
                sessionTimeInterval: '10:00 - 17:00',
              },
            ],
            totalAppointments: 420,
            booked: 2,
            unbooked: 418,
          },
        ],
      );
    });

    test('Clicking into the change BST session has the correct information, for the edit session decision page', async ({
      page,
    }) => {
      const changeButton = page
        .getByRole('heading', {
          name: 'Saturday 25 October',
        })
        .locator('..')
        .getByRole('link', {
          name: 'Change',
        });

      await changeButton.click();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-25&session**`,
      );

      expect(changeAvailabilityPage.changeHeader).toHaveText(
        'Church Lane PharmacyChange availability for 25 October 2025',
      );

      //table headers!
      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', { name: 'Time' }),
      ).toBeVisible();

      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Services',
        }),
      ).toBeVisible();

      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Booked',
          exact: true,
        }),
      ).toBeVisible();

      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Unbooked',
          exact: true,
        }),
      ).toBeVisible();

      //no action header
      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Action',
        }),
      ).not.toBeVisible();

      const timeCell = changeAvailabilityPage.page.getByRole('cell', {
        name: '10:00 - 17:00',
      });
      const serviceCell = changeAvailabilityPage.page.getByRole('cell', {
        name: 'RSV (Adult)',
      });
      const bookedCell = changeAvailabilityPage.page.getByRole('cell', {
        name: '2 booked',
      });
      const unbookedCell = changeAvailabilityPage.page.getByRole('cell', {
        name: '418 unbooked',
      });

      await expect(timeCell).toBeVisible();
      await expect(serviceCell).toBeVisible();
      await expect(bookedCell).toBeVisible();
      await expect(unbookedCell).toBeVisible();
    });

    test('Clicking into the change BST session, and clicking through to the edit session page', async ({
      page,
    }) => {
      const changeButton = page
        .getByRole('heading', {
          name: 'Saturday 25 October',
        })
        .locator('..')
        .getByRole('link', {
          name: 'Change',
        });

      await changeButton.click();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-25&session**`,
      );

      await expect(changeAvailabilityPage.changeHeader).toHaveText(
        'Church Lane PharmacyChange availability for 25 October 2025',
      );

      await changeAvailabilityPage.selectChangeType('ChangeLengthCapacity');
      await changeAvailabilityPage.saveChanges();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/availability/edit?session**`,
      );

      await expect(addSessionPage.addSessionHeader).toHaveText(
        'Edit sessionEdit time and capacity for 25 October 2025',
      );

      await expect(
        addSessionPage.page.getByText('2 booked appointments in this session.'),
      ).toBeVisible();
      await expect(
        addSessionPage.page.getByText(
          '418 unbooked appointments in this session.',
        ),
      ).toBeVisible();

      await expect(addSessionPage.startTimeHour).toHaveValue('10');
      await expect(addSessionPage.startTimeMinute).toHaveValue('00');
      await expect(addSessionPage.endTimeHour).toHaveValue('17');
      await expect(addSessionPage.endTimeMinute).toHaveValue('00');
    });

    test('Clicking into the change BST session, and clicking through to the cancel session page', async ({
      page,
    }) => {
      const changeButton = page
        .getByRole('heading', {
          name: 'Saturday 25 October',
        })
        .locator('..')
        .getByRole('link', {
          name: 'Change',
        });

      await changeButton.click();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-25&session**`,
      );

      await expect(changeAvailabilityPage.changeHeader).toHaveText(
        'Church Lane PharmacyChange availability for 25 October 2025',
      );

      await changeAvailabilityPage.selectChangeType('CancelSession');
      await changeAvailabilityPage.saveChanges();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/availability/cancel?session**`,
      );

      await expect(cancelSessionPage.cancelSessionHeader).toHaveText(
        'Cancel sessionAre you sure you want to cancel this session?',
      );

      //single table
      await expect(cancelSessionPage.page.locator('table')).toHaveCount(1);

      //table headers!
      await expect(
        cancelSessionPage.page.getByRole('columnheader', { name: 'Time' }),
      ).toBeVisible();

      await expect(
        cancelSessionPage.page.getByRole('columnheader', { name: 'Services' }),
      ).toBeVisible();

      await expect(
        cancelSessionPage.page.getByRole('columnheader', {
          name: 'Booked',
          exact: true,
        }),
      ).toBeVisible();

      await expect(
        cancelSessionPage.page.getByRole('columnheader', {
          name: 'Unbooked',
          exact: true,
        }),
      ).toBeVisible();

      //no actions
      await expect(
        cancelSessionPage.page.getByRole('columnheader', { name: 'Action' }),
      ).not.toBeVisible();

      const timeCell = cancelSessionPage.page.getByRole('cell', {
        name: '10:00 - 17:00',
      });
      const serviceCell = cancelSessionPage.page.getByRole('cell', {
        name: 'RSV (Adult)',
      });
      const bookedCell = cancelSessionPage.page.getByRole('cell', {
        name: '2 booked',
      });
      const unbookedCell = cancelSessionPage.page.getByRole('cell', {
        name: '418 unbooked',
      });

      await expect(timeCell).toBeVisible();
      await expect(serviceCell).toBeVisible();
      await expect(bookedCell).toBeVisible();
      await expect(unbookedCell).toBeVisible();
    });

    test('Clicking into the change UTC session has the correct information, for the edit session decision page', async ({
      page,
    }) => {
      const changeButton = page
        .getByRole('heading', {
          name: 'Sunday 26 October',
        })
        .locator('..')
        .getByRole('link', {
          name: 'Change',
        });

      await changeButton.click();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
      );

      expect(changeAvailabilityPage.changeHeader).toHaveText(
        'Church Lane PharmacyChange availability for 26 October 2025',
      );

      //table headers!
      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', { name: 'Time' }),
      ).toBeVisible();

      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Services',
        }),
      ).toBeVisible();

      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Booked',
          exact: true,
        }),
      ).toBeVisible();

      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Unbooked',
          exact: true,
        }),
      ).toBeVisible();

      //no action header
      await expect(
        changeAvailabilityPage.page.getByRole('columnheader', {
          name: 'Action',
        }),
      ).not.toBeVisible();

      const timeCell = changeAvailabilityPage.page.getByRole('cell', {
        name: '10:00 - 17:00',
      });
      const serviceCell = changeAvailabilityPage.page.getByRole('cell', {
        name: 'RSV (Adult)',
      });
      const bookedCell = changeAvailabilityPage.page.getByRole('cell', {
        name: '2 booked',
      });
      const unbookedCell = changeAvailabilityPage.page.getByRole('cell', {
        name: '418 unbooked',
      });

      await expect(timeCell).toBeVisible();
      await expect(serviceCell).toBeVisible();
      await expect(bookedCell).toBeVisible();
      await expect(unbookedCell).toBeVisible();
    });

    test('Clicking into the change UTC session, and clicking through to the edit session page', async ({
      page,
    }) => {
      const changeButton = page
        .getByRole('heading', {
          name: 'Sunday 26 October',
        })
        .locator('..')
        .getByRole('link', {
          name: 'Change',
        });

      await changeButton.click();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
      );

      await expect(changeAvailabilityPage.changeHeader).toHaveText(
        'Church Lane PharmacyChange availability for 26 October 2025',
      );

      await changeAvailabilityPage.selectChangeType('ChangeLengthCapacity');
      await changeAvailabilityPage.saveChanges();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/availability/edit?session**`,
      );

      await expect(addSessionPage.addSessionHeader).toHaveText(
        'Edit sessionEdit time and capacity for 26 October 2025',
      );

      await expect(
        addSessionPage.page.getByText('2 booked appointments in this session.'),
      ).toBeVisible();
      await expect(
        addSessionPage.page.getByText(
          '418 unbooked appointments in this session.',
        ),
      ).toBeVisible();

      await expect(addSessionPage.startTimeHour).toHaveValue('10');
      await expect(addSessionPage.startTimeMinute).toHaveValue('00');
      await expect(addSessionPage.endTimeHour).toHaveValue('17');
      await expect(addSessionPage.endTimeMinute).toHaveValue('00');
    });

    test('Clicking into the change UTC session, and clicking through to the cancel session page', async ({
      page,
    }) => {
      const changeButton = page
        .getByRole('heading', {
          name: 'Sunday 26 October',
        })
        .locator('..')
        .getByRole('link', {
          name: 'Change',
        });

      await changeButton.click();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
      );

      await expect(changeAvailabilityPage.changeHeader).toHaveText(
        'Church Lane PharmacyChange availability for 26 October 2025',
      );

      await changeAvailabilityPage.selectChangeType('CancelSession');
      await changeAvailabilityPage.saveChanges();

      await page.waitForURL(
        `manage-your-appointments/site/${site.id}/availability/cancel?session**`,
      );

      await expect(cancelSessionPage.cancelSessionHeader).toHaveText(
        'Cancel sessionAre you sure you want to cancel this session?',
      );

      //single table
      await expect(cancelSessionPage.page.locator('table')).toHaveCount(1);

      //table headers!
      await expect(
        cancelSessionPage.page.getByRole('columnheader', { name: 'Time' }),
      ).toBeVisible();

      await expect(
        cancelSessionPage.page.getByRole('columnheader', { name: 'Services' }),
      ).toBeVisible();

      await expect(
        cancelSessionPage.page.getByRole('columnheader', {
          name: 'Booked',
          exact: true,
        }),
      ).toBeVisible();

      await expect(
        cancelSessionPage.page.getByRole('columnheader', {
          name: 'Unbooked',
          exact: true,
        }),
      ).toBeVisible();

      //no actions
      await expect(
        cancelSessionPage.page.getByRole('columnheader', { name: 'Action' }),
      ).not.toBeVisible();

      const timeCell = cancelSessionPage.page.getByRole('cell', {
        name: '10:00 - 17:00',
      });
      const serviceCell = cancelSessionPage.page.getByRole('cell', {
        name: 'RSV (Adult)',
      });
      const bookedCell = cancelSessionPage.page.getByRole('cell', {
        name: '2 booked',
      });
      const unbookedCell = cancelSessionPage.page.getByRole('cell', {
        name: '418 unbooked',
      });

      await expect(timeCell).toBeVisible();
      await expect(serviceCell).toBeVisible();
      await expect(bookedCell).toBeVisible();
      await expect(unbookedCell).toBeVisible();
    });
  },
);
