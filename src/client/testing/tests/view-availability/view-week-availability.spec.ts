import {
  AddSessionPage,
  CancelSessionDetailsPage,
  ChangeAvailabilityPage,
  DailyAppointmentDetailsPage,
  OAuthLoginPage,
  RootPage,
  WeekViewAvailabilityPage,
  CancelAppointmentDetailsPage,
} from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let viewWeekAvailabilityPage: WeekViewAvailabilityPage;
let changeAvailabilityPage: ChangeAvailabilityPage;
let addSessionPage: AddSessionPage;
let cancelSessionPage: CancelSessionDetailsPage;
let dailyAppointmentDetailsPage: DailyAppointmentDetailsPage;
let cancelAppointmentDetailsPage: CancelAppointmentDetailsPage;
let site: Site;

type SessionTestCase = {
  week: string;
  day: string;
  dayCardHeader: string;
  changeSessionHeader: string;
  timeRange: string;
  startHour: string;
  startMins: string;
  endHour: string;
  endMins: string;
  service: string;
  booked: number;
  unbooked: number;
  //the contents of these two SHOULD be identical, but there seems to be some indiscrepancies over these pages...
  viewDailyAppointments: ViewDailyAppointment[];
  cancelDailyAppointments: CancelDailyAppointment[];
};

type ViewDailyAppointment = {
  time: string;
  nameNhsNumber: string;
  dob: string;
  contactDetails: string;
  services: string;
};

type CancelDailyAppointment = {
  time: string;
  name: string;
  nhsNumber: string;
  dob: string;
  contactDetails: string;
  services: string;
};

//session test cases to verify, should have session&booking data in the seeder
const sessionTestCases: SessionTestCase[] = [
  {
    week: '2025-10-20',
    day: '2025-10-25',
    dayCardHeader: 'Saturday 25 October',
    changeSessionHeader: '25 October 2025',
    timeRange: '10:00 - 17:00',
    startHour: '10',
    startMins: '00',
    endHour: '17',
    endMins: '00',
    service: 'RSV (Adult)',
    booked: 2,
    unbooked: 418,
    viewDailyAppointments: [
      {
        time: '10:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '25 October 202510:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
      {
        time: '25 October 202516:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
    ],
  },
  {
    week: '2025-10-20',
    day: '2025-10-26',
    dayCardHeader: 'Sunday 26 October',
    changeSessionHeader: '26 October 2025',
    timeRange: '10:00 - 17:00',
    startHour: '10',
    startMins: '00',
    endHour: '17',
    endMins: '00',
    service: 'RSV (Adult)',
    booked: 2,
    unbooked: 418,
    viewDailyAppointments: [
      {
        time: '10:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
      {
        time: '16:55',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '26 October 202510:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
      {
        time: '26 October 202516:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
    ],
  },
];

test.describe('View Week Availability', () => {
  test.beforeEach(async ({ page, getTestSite }) => {
    site = getTestSite(2);
    rootPage = new RootPage(page);
    oAuthPage = new OAuthLoginPage(page);
    viewWeekAvailabilityPage = new WeekViewAvailabilityPage(page);
    changeAvailabilityPage = new ChangeAvailabilityPage(page);
    addSessionPage = new AddSessionPage(page);
    cancelSessionPage = new CancelSessionDetailsPage(page);
    dailyAppointmentDetailsPage = new DailyAppointmentDetailsPage(page);
    cancelAppointmentDetailsPage = new CancelAppointmentDetailsPage(page);

    await rootPage.goto();
    await rootPage.pageContentLogInButton.click();
    await oAuthPage.signIn();
  });

  test('All the view week page data is arranged in the day cards as expected for Oct 20-27th 2025', async ({
    page,
  }) => {
    //go to a week page that has a daylight savings change
    await page.goto(
      `manage-your-appointments/site/${site.id}/view-availability/week?date=2025-10-20`,
    );

    await viewWeekAvailabilityPage.verifyViewNextAndPreviousWeeksButtonsDisplayed(
      '13-19 October 2025',
      '27-2 November 2025',
    );
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

  sessionTestCases.forEach(daySession => {
    test.describe(`Session tests for day: '${daySession.dayCardHeader}'`, () => {
      test.beforeEach(async ({ page }) => {
        //start test by navigating to the week view that contains this session
        await page.goto(
          `manage-your-appointments/site/${site.id}/view-availability/week?date=${daySession.week}`,
        );
      });

      test('Change session has the correct information on the edit session decision page', async ({
        page,
      }) => {
        const changeButton = page
          .getByRole('heading', {
            name: daySession.dayCardHeader,
          })
          .locator('..')
          .getByRole('link', {
            name: 'Change',
          });

        await changeButton.click();

        await page.waitForURL(
          `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
        );

        expect(changeAvailabilityPage.changeHeader).toHaveText(
          `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
        );

        //table headers!
        await expect(
          changeAvailabilityPage.page.getByRole('columnheader', {
            name: 'Time',
          }),
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
          name: daySession.timeRange,
        });
        const serviceCell = changeAvailabilityPage.page.getByRole('cell', {
          name: daySession.service,
        });
        const bookedCell = changeAvailabilityPage.page.getByRole('cell', {
          name: `${daySession.booked} booked`,
        });
        const unbookedCell = changeAvailabilityPage.page.getByRole('cell', {
          name: `${daySession.unbooked} unbooked`,
        });

        await expect(timeCell).toBeVisible();
        await expect(serviceCell).toBeVisible();
        await expect(bookedCell).toBeVisible();
        await expect(unbookedCell).toBeVisible();
      });

      test('Change session has the correct information on the edit session page', async ({
        page,
      }) => {
        const changeButton = page
          .getByRole('heading', {
            name: daySession.dayCardHeader,
          })
          .locator('..')
          .getByRole('link', {
            name: 'Change',
          });

        await changeButton.click();

        await page.waitForURL(
          `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
        );

        expect(changeAvailabilityPage.changeHeader).toHaveText(
          `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
        );

        await changeAvailabilityPage.selectChangeType('ChangeLengthCapacity');
        await changeAvailabilityPage.saveChanges();

        await page.waitForURL(
          `manage-your-appointments/site/${site.id}/availability/edit?session**`,
        );

        await expect(addSessionPage.addSessionHeader).toHaveText(
          `Edit sessionEdit time and capacity for ${daySession.changeSessionHeader}`,
        );

        await expect(
          addSessionPage.page.getByText(
            `${daySession.booked} booked appointments in this session.`,
          ),
        ).toBeVisible();
        await expect(
          addSessionPage.page.getByText(
            `${daySession.unbooked} unbooked appointments in this session.`,
          ),
        ).toBeVisible();

        await expect(addSessionPage.startTimeHour).toHaveValue(
          daySession.startHour,
        );
        await expect(addSessionPage.startTimeMinute).toHaveValue(
          daySession.startMins,
        );
        await expect(addSessionPage.endTimeHour).toHaveValue(
          daySession.endHour,
        );
        await expect(addSessionPage.endTimeMinute).toHaveValue(
          daySession.endMins,
        );
      });

      test('Change session has the correct information on the cancel session page', async ({
        page,
      }) => {
        const changeButton = page
          .getByRole('heading', {
            name: daySession.dayCardHeader,
          })
          .locator('..')
          .getByRole('link', {
            name: 'Change',
          });

        await changeButton.click();

        await page.waitForURL(
          `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=${daySession.day}&session**`,
        );

        expect(changeAvailabilityPage.changeHeader).toHaveText(
          `Church Lane PharmacyChange availability for ${daySession.changeSessionHeader}`,
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
          cancelSessionPage.page.getByRole('columnheader', {
            name: 'Services',
          }),
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
          name: daySession.timeRange,
        });
        const serviceCell = cancelSessionPage.page.getByRole('cell', {
          name: daySession.service,
        });
        const bookedCell = cancelSessionPage.page.getByRole('cell', {
          name: `${daySession.booked} booked`,
        });
        const unbookedCell = cancelSessionPage.page.getByRole('cell', {
          name: `${daySession.unbooked} unbooked`,
        });

        await expect(timeCell).toBeVisible();
        await expect(serviceCell).toBeVisible();
        await expect(bookedCell).toBeVisible();
        await expect(unbookedCell).toBeVisible();
      });

      test('View daily appointments has the correct information, and on the cancel appointment page', async ({
        page,
      }) => {
        const viewDailyAppointmentsButton = page
          .getByRole('heading', {
            name: daySession.dayCardHeader,
          })
          .locator('..')
          .getByRole('link', {
            name: 'View daily appointments',
          });

        await viewDailyAppointmentsButton.click();

        await page.waitForURL(
          `manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
        );

        dailyAppointmentDetailsPage.verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
          daySession.viewDailyAppointments,
        );

        const allTableRows = await dailyAppointmentDetailsPage.appointmentsTable
          .getByRole('row')
          .all();

        //dive into the cancel details page and verify information is correct
        for (
          let index = 0;
          index < daySession.cancelDailyAppointments.length;
          index++
        ) {
          const expectedAppointment = daySession.cancelDailyAppointments[index];

          //start at 1 to ignore header row
          const tableRow = allTableRows[index + 1];

          const cancelLink = tableRow.getByRole('link', { name: 'Cancel' });

          await expect(cancelLink).toBeEnabled();

          await cancelLink.click();

          await page.waitForURL(
            `manage-your-appointments/site/${site.id}/appointment/**/cancel`,
          );

          await cancelAppointmentDetailsPage.verifyAppointmentDetailsDisplayed(
            expectedAppointment,
          );

          //need to go back after this check
          await cancelAppointmentDetailsPage.backButton.click();

          await page.waitForURL(
            `manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=${daySession.day}&page=1`,
          );
        }
      });
    });
  });
  //   test('Change session has the correct information on the edit session decision page', async ({
  //     page,
  //   }) => {
  //     const changeButton = page
  //       .getByRole('heading', {
  //         name: 'Sunday 26 October',
  //       })
  //       .locator('..')
  //       .getByRole('link', {
  //         name: 'Change',
  //       });

  //     await changeButton.click();

  //     await page.waitForURL(
  //       `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
  //     );

  //     expect(changeAvailabilityPage.changeHeader).toHaveText(
  //       'Church Lane PharmacyChange availability for 26 October 2025',
  //     );

  //     //table headers!
  //     await expect(
  //       changeAvailabilityPage.page.getByRole('columnheader', { name: 'Time' }),
  //     ).toBeVisible();

  //     await expect(
  //       changeAvailabilityPage.page.getByRole('columnheader', {
  //         name: 'Services',
  //       }),
  //     ).toBeVisible();

  //     await expect(
  //       changeAvailabilityPage.page.getByRole('columnheader', {
  //         name: 'Booked',
  //         exact: true,
  //       }),
  //     ).toBeVisible();

  //     await expect(
  //       changeAvailabilityPage.page.getByRole('columnheader', {
  //         name: 'Unbooked',
  //         exact: true,
  //       }),
  //     ).toBeVisible();

  //     //no action header
  //     await expect(
  //       changeAvailabilityPage.page.getByRole('columnheader', {
  //         name: 'Action',
  //       }),
  //     ).not.toBeVisible();

  //     const timeCell = changeAvailabilityPage.page.getByRole('cell', {
  //       name: '10:00 - 17:00',
  //     });
  //     const serviceCell = changeAvailabilityPage.page.getByRole('cell', {
  //       name: 'RSV (Adult)',
  //     });
  //     const bookedCell = changeAvailabilityPage.page.getByRole('cell', {
  //       name: '2 booked',
  //     });
  //     const unbookedCell = changeAvailabilityPage.page.getByRole('cell', {
  //       name: '418 unbooked',
  //     });

  //     await expect(timeCell).toBeVisible();
  //     await expect(serviceCell).toBeVisible();
  //     await expect(bookedCell).toBeVisible();
  //     await expect(unbookedCell).toBeVisible();
  //   });

  //   test('Change session has the correct information on the edit session page', async ({
  //     page,
  //   }) => {
  //     const changeButton = page
  //       .getByRole('heading', {
  //         name: 'Sunday 26 October',
  //       })
  //       .locator('..')
  //       .getByRole('link', {
  //         name: 'Change',
  //       });

  //     await changeButton.click();

  //     await page.waitForURL(
  //       `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
  //     );

  //     await expect(changeAvailabilityPage.changeHeader).toHaveText(
  //       'Church Lane PharmacyChange availability for 26 October 2025',
  //     );

  //     await changeAvailabilityPage.selectChangeType('ChangeLengthCapacity');
  //     await changeAvailabilityPage.saveChanges();

  //     await page.waitForURL(
  //       `manage-your-appointments/site/${site.id}/availability/edit?session**`,
  //     );

  //     await expect(addSessionPage.addSessionHeader).toHaveText(
  //       'Edit sessionEdit time and capacity for 26 October 2025',
  //     );

  //     await expect(
  //       addSessionPage.page.getByText('2 booked appointments in this session.'),
  //     ).toBeVisible();
  //     await expect(
  //       addSessionPage.page.getByText(
  //         '418 unbooked appointments in this session.',
  //       ),
  //     ).toBeVisible();

  //     await expect(addSessionPage.startTimeHour).toHaveValue('10');
  //     await expect(addSessionPage.startTimeMinute).toHaveValue('00');
  //     await expect(addSessionPage.endTimeHour).toHaveValue('17');
  //     await expect(addSessionPage.endTimeMinute).toHaveValue('00');
  //   });

  //   test('Change session has the correct information on the cancel session page', async ({
  //     page,
  //   }) => {
  //     const changeButton = page
  //       .getByRole('heading', {
  //         name: 'Sunday 26 October',
  //       })
  //       .locator('..')
  //       .getByRole('link', {
  //         name: 'Change',
  //       });

  //     await changeButton.click();

  //     await page.waitForURL(
  //       `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
  //     );

  //     await expect(changeAvailabilityPage.changeHeader).toHaveText(
  //       'Church Lane PharmacyChange availability for 26 October 2025',
  //     );

  //     await changeAvailabilityPage.selectChangeType('CancelSession');
  //     await changeAvailabilityPage.saveChanges();

  //     await page.waitForURL(
  //       `manage-your-appointments/site/${site.id}/availability/cancel?session**`,
  //     );

  //     await expect(cancelSessionPage.cancelSessionHeader).toHaveText(
  //       'Cancel sessionAre you sure you want to cancel this session?',
  //     );

  //     //single table
  //     await expect(cancelSessionPage.page.locator('table')).toHaveCount(1);

  //     //table headers!
  //     await expect(
  //       cancelSessionPage.page.getByRole('columnheader', { name: 'Time' }),
  //     ).toBeVisible();

  //     await expect(
  //       cancelSessionPage.page.getByRole('columnheader', { name: 'Services' }),
  //     ).toBeVisible();

  //     await expect(
  //       cancelSessionPage.page.getByRole('columnheader', {
  //         name: 'Booked',
  //         exact: true,
  //       }),
  //     ).toBeVisible();

  //     await expect(
  //       cancelSessionPage.page.getByRole('columnheader', {
  //         name: 'Unbooked',
  //         exact: true,
  //       }),
  //     ).toBeVisible();

  //     //no actions
  //     await expect(
  //       cancelSessionPage.page.getByRole('columnheader', { name: 'Action' }),
  //     ).not.toBeVisible();

  //     const timeCell = cancelSessionPage.page.getByRole('cell', {
  //       name: '10:00 - 17:00',
  //     });
  //     const serviceCell = cancelSessionPage.page.getByRole('cell', {
  //       name: 'RSV (Adult)',
  //     });
  //     const bookedCell = cancelSessionPage.page.getByRole('cell', {
  //       name: '2 booked',
  //     });
  //     const unbookedCell = cancelSessionPage.page.getByRole('cell', {
  //       name: '418 unbooked',
  //     });

  //     await expect(timeCell).toBeVisible();
  //     await expect(serviceCell).toBeVisible();
  //     await expect(bookedCell).toBeVisible();
  //     await expect(unbookedCell).toBeVisible();
  //   });

  //   test('View daily appointments has the correct information, and on the cancel appointment page', async ({
  //     page,
  //   }) => {
  //     const viewDailyAppointmentsButton = page
  //       .getByRole('heading', {
  //         name: 'Sunday 26 October',
  //       })
  //       .locator('..')
  //       .getByRole('link', {
  //         name: 'View daily appointments',
  //       });

  //     await viewDailyAppointmentsButton.click();

  //     await page.waitForURL(
  //       `manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=2025-10-26&page=1`,
  //     );

  //     const expectedDailyAppointments = ;

  //     dailyAppointmentDetailsPage.verifyAllDailyAppointmentsTableInformationDisplayedCorrectly(
  //       expectedDailyAppointments,
  //     );

  //     const expectedCancellableAppointments = ;

  //     const allTableRows = await dailyAppointmentDetailsPage.appointmentsTable
  //       .getByRole('row')
  //       .all();

  //     //dive into the cancel details page and verify information is correct
  //     for (
  //       let index = 0;
  //       index < expectedCancellableAppointments.length;
  //       index++
  //     ) {
  //       const expectedAppointment = expectedCancellableAppointments[index];

  //       //start at 1 to ignore header row
  //       const tableRow = allTableRows[index + 1];

  //       const cancelLink = tableRow.getByRole('link', { name: 'Cancel' });

  //       await expect(cancelLink).toBeEnabled();

  //       await cancelLink.click();

  //       await page.waitForURL(
  //         `manage-your-appointments/site/${site.id}/appointment/**/cancel`,
  //       );

  //       await cancelAppointmentDetailsPage.verifyAppointmentDetailsDisplayed(
  //         expectedAppointment,
  //       );

  //       //need to go back after this check
  //       await cancelAppointmentDetailsPage.backButton.click();

  //       await page.waitForURL(
  //         `manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=2025-10-26&page=1`,
  //       );
  //     }
  //   });
  // });
});
