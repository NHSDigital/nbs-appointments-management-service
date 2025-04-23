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
import { DayOverview } from '../../page-objects/view-availability-appointment-pages/week-view-availability-page';

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

type WeekViewTestCase = {
  week: string;
  weekHeader: string;
  previousWeek: string;
  nextWeek: string;
  dayOverviews: DayOverview[];
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
  {
    week: '2025-10-27',
    day: '2025-10-27',
    dayCardHeader: 'Monday 27 October',
    changeSessionHeader: '27 October 2025',
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
        time: '27 October 202510:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
      {
        time: '27 October 202516:55pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
    ],
  },
  {
    week: '2026-03-23',
    day: '2026-03-28',
    dayCardHeader: 'Saturday 28 March',
    changeSessionHeader: '28 March 2026',
    timeRange: '08:00 - 14:00',
    startHour: '08',
    startMins: '00',
    endHour: '14',
    endMins: '00',
    service: 'RSV (Adult)',
    booked: 2,
    unbooked: 238,
    viewDailyAppointments: [
      {
        time: '08:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '28 March 20268:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
      {
        time: '28 March 202613:45pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
    ],
  },
  {
    week: '2026-03-23',
    day: '2026-03-29',
    dayCardHeader: 'Sunday 29 March',
    changeSessionHeader: '29 March 2026',
    timeRange: '08:00 - 14:00',
    startHour: '08',
    startMins: '00',
    endHour: '14',
    endMins: '00',
    service: 'RSV (Adult)',
    booked: 2,
    unbooked: 238,
    viewDailyAppointments: [
      {
        time: '08:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '29 March 20268:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
      {
        time: '29 March 202613:45pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
    ],
  },
  {
    week: '2026-03-30',
    day: '2026-03-30',
    dayCardHeader: 'Monday 30 March',
    changeSessionHeader: '30 March 2026',
    timeRange: '08:00 - 14:00',
    startHour: '08',
    startMins: '00',
    endHour: '14',
    endMins: '00',
    service: 'RSV (Adult)',
    booked: 2,
    unbooked: 238,
    viewDailyAppointments: [
      {
        time: '08:00',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
      {
        time: '13:45',
        nameNhsNumber: 'Jeremy Oswald1975486535',
        dob: '13 November 1952',
        contactDetails: '',
        services: 'RSV',
      },
    ],
    cancelDailyAppointments: [
      {
        time: '30 March 20268:00am',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
      {
        time: '30 March 202613:45pm',
        name: 'Jeremy Oswald',
        nhsNumber: '1975486535',
        dob: '13 November 1952',
        contactDetails: 'Not provided',
        services: 'RSV (Adult)',
      },
    ],
  },
];

const weekTestCases: WeekViewTestCase[] = [
  {
    week: '2025-10-20',
    weekHeader: '20 October to 26 October',
    previousWeek: '13-19 October 2025',
    nextWeek: '27-2 November 2025',
    dayOverviews: [
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
  },
  {
    week: '2025-10-27',
    weekHeader: '27 October to 2 November',
    previousWeek: '20-26 October 2025',
    nextWeek: '3-9 November 2025',
    dayOverviews: [
      {
        header: 'Monday 27 October',
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
        header: 'Tuesday 28 October',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Wednesday 29 October',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Thursday 30 October',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Friday 31 October',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Saturday 1 November',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Sunday 2 November',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
    ],
  },
  {
    week: '2026-03-23',
    weekHeader: '23 March to 29 March',
    previousWeek: '16-22 March 2026',
    nextWeek: '30-5 April 2026',
    dayOverviews: [
      {
        header: 'Monday 23 March',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Tuesday 24 March',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Wednesday 25 March',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Thursday 26 March',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Friday 27 March',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Saturday 28 March',
        services: [
          {
            serviceName: 'RSV (Adult)',
            booked: 2,
            unbooked: 238,
            sessionTimeInterval: '08:00 - 14:00',
          },
        ],
        totalAppointments: 240,
        booked: 2,
        unbooked: 238,
      },
      {
        header: 'Sunday 29 March',
        services: [
          {
            serviceName: 'RSV (Adult)',
            booked: 2,
            unbooked: 238,
            sessionTimeInterval: '08:00 - 14:00',
          },
        ],
        totalAppointments: 240,
        booked: 2,
        unbooked: 238,
      },
    ],
  },
  {
    week: '2026-03-30',
    weekHeader: '30 March to 5 April',
    previousWeek: '23-29 March 2026',
    nextWeek: '6-12 April 2026',
    dayOverviews: [
      {
        header: 'Monday 30 March',
        services: [
          {
            serviceName: 'RSV (Adult)',
            booked: 2,
            unbooked: 238,
            sessionTimeInterval: '08:00 - 14:00',
          },
        ],
        totalAppointments: 240,
        booked: 2,
        unbooked: 238,
      },
      {
        header: 'Tuesday 31 March',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Wednesday 1 April',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Thursday 2 April',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Friday 3 April',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Saturday 4 April',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
      {
        header: 'Sunday 5 April',
        services: [],
        totalAppointments: 0,
        booked: 0,
        unbooked: 0,
      },
    ],
  },
];

['UTC', 'Europe/London', 'Pacific/Kiritimati', 'Etc/GMT+12'].forEach(
  timezone => {
    test.describe(`Test in timezone: '${timezone}'`, () => {
      test.use({ timezoneId: timezone });

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

        weekTestCases.forEach(week => {
          test.describe(`Session tests for week: '${week.weekHeader}'`, () => {
            test.beforeEach(async ({ page }) => {
              //start test by navigating to the week view that contains this session
              await page.goto(
                `manage-your-appointments/site/${site.id}/view-availability/week?date=${week.week}`,
              );
            });

            test(`View week page data is arranged in the day cards as expected`, async () => {
              await viewWeekAvailabilityPage.verifyViewNextAndPreviousWeeksButtonsDisplayed(
                week.previousWeek,
                week.nextWeek,
              );
              await viewWeekAvailabilityPage.verifyAllDayCardInformationDisplayedCorrectly(
                week.dayOverviews,
              );
            });
          });
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
              const serviceCell = changeAvailabilityPage.page.getByRole(
                'cell',
                {
                  name: daySession.service,
                },
              );
              const bookedCell = changeAvailabilityPage.page.getByRole('cell', {
                name: `${daySession.booked} booked`,
              });
              const unbookedCell = changeAvailabilityPage.page.getByRole(
                'cell',
                {
                  name: `${daySession.unbooked} unbooked`,
                },
              );

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

              await changeAvailabilityPage.selectChangeType(
                'ChangeLengthCapacity',
              );
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
              await expect(cancelSessionPage.page.locator('table')).toHaveCount(
                1,
              );

              //table headers!
              await expect(
                cancelSessionPage.page.getByRole('columnheader', {
                  name: 'Time',
                }),
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
                cancelSessionPage.page.getByRole('columnheader', {
                  name: 'Action',
                }),
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

              const allTableRows =
                await dailyAppointmentDetailsPage.appointmentsTable
                  .getByRole('row')
                  .all();

              //dive into the cancel details page and verify information is correct
              for (
                let index = 0;
                index < daySession.cancelDailyAppointments.length;
                index++
              ) {
                const expectedAppointment =
                  daySession.cancelDailyAppointments[index];

                //start at 1 to ignore header row
                const tableRow = allTableRows[index + 1];

                const cancelLink = tableRow.getByRole('link', {
                  name: 'Cancel',
                });

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
      });
    });
  },
);
