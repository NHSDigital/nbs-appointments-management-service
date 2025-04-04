import { test, expect } from '../../fixtures';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import { Site } from '@types';
import ViewWeekAvailabilityPage from '../../page-objects/view-availability-appointment-pages/view-week-availability-page';
import EditSessionDecisionPage from '../../page-objects/edit-availability/edit-session-decision-page';
import EditSessionPage from '../../page-objects/edit-availability/edit-session-page';
import CancelSessionPage from '../../page-objects/edit-availability/cancel-session-page';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let viewWeekAvailabilityPage: ViewWeekAvailabilityPage;
let editSessionDecisionPage: EditSessionDecisionPage;
let editSessionPage: EditSessionPage;
let cancelSessionPage: CancelSessionPage;
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
  editSessionDecisionPage = new EditSessionDecisionPage(page);
  editSessionPage = new EditSessionPage(page);
  cancelSessionPage = new CancelSessionPage(page);

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

test('Clicking into the change BST session has the correct information, for the edit session decision page', async ({
  page,
}) => {
  await viewWeekAvailabilityPage.changeButtons[0].click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-25&session**`,
  );

  expect(editSessionDecisionPage.changeHeader).toHaveText(
    'Church Lane PharmacyChange availability for 25 October 2025',
  );

  //table headers!
  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', { name: 'Time' }),
  ).toBeVisible();

  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', {
      name: 'Services',
    }),
  ).toBeVisible();

  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', {
      name: 'Booked',
      exact: true,
    }),
  ).toBeVisible();

  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', {
      name: 'Unbooked',
      exact: true,
    }),
  ).toBeVisible();

  //no action header
  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', { name: 'Action' }),
  ).not.toBeVisible();

  const timeCell = editSessionDecisionPage.page.getByRole('cell', {
    name: '10:00 - 17:00',
  });
  const serviceCell = editSessionDecisionPage.page.getByRole('cell', {
    name: 'RSV (Adult)',
  });
  const bookedCell = editSessionDecisionPage.page.getByRole('cell', {
    name: '0 booked',
  });
  const unbookedCell = editSessionDecisionPage.page.getByRole('cell', {
    name: '420 unbooked',
  });

  await expect(timeCell).toBeVisible();
  await expect(serviceCell).toBeVisible();
  await expect(bookedCell).toBeVisible();
  await expect(unbookedCell).toBeVisible();
});

test('Clicking into the change BST session, and clicking through to the edit session page', async ({
  page,
}) => {
  await viewWeekAvailabilityPage.changeButtons[0].click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-25&session**`,
  );

  await expect(editSessionDecisionPage.changeHeader).toHaveText(
    'Church Lane PharmacyChange availability for 25 October 2025',
  );

  await editSessionDecisionPage.editLengthCapacityRadioOption.click();
  await editSessionDecisionPage.continueButton.click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/availability/edit?session**`,
  );

  await expect(editSessionPage.editSessionHeader).toHaveText(
    'Edit sessionEdit time and capacity for 25 October 2025',
  );

  await expect(
    editSessionPage.page.getByText('0 booked appointments in this session.'),
  ).toBeVisible();
  await expect(
    editSessionPage.page.getByText(
      '420 unbooked appointments in this session.',
    ),
  ).toBeVisible();

  await expect(editSessionPage.startHourInput).toHaveValue('10');
  await expect(editSessionPage.startMinuteInput).toHaveValue('00');
  await expect(editSessionPage.endHourInput).toHaveValue('17');
  await expect(editSessionPage.endMinuteInput).toHaveValue('00');
});

test('Clicking into the change BST session, and clicking through to the cancel session page', async ({
  page,
}) => {
  await viewWeekAvailabilityPage.changeButtons[0].click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-25&session**`,
  );

  await expect(editSessionDecisionPage.changeHeader).toHaveText(
    'Church Lane PharmacyChange availability for 25 October 2025',
  );

  await editSessionDecisionPage.cancelRadioOption.click();
  await editSessionDecisionPage.continueButton.click();

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
    name: '0 booked',
  });
  const unbookedCell = cancelSessionPage.page.getByRole('cell', {
    name: '420 unbooked',
  });

  await expect(timeCell).toBeVisible();
  await expect(serviceCell).toBeVisible();
  await expect(bookedCell).toBeVisible();
  await expect(unbookedCell).toBeVisible();
});

test('Clicking into the change UTC session has the correct information, for the edit session decision page', async ({
  page,
}) => {
  await viewWeekAvailabilityPage.changeButtons[1].click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
  );

  expect(editSessionDecisionPage.changeHeader).toHaveText(
    'Church Lane PharmacyChange availability for 26 October 2025',
  );

  //table headers!
  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', { name: 'Time' }),
  ).toBeVisible();

  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', {
      name: 'Services',
    }),
  ).toBeVisible();

  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', {
      name: 'Booked',
      exact: true,
    }),
  ).toBeVisible();

  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', {
      name: 'Unbooked',
      exact: true,
    }),
  ).toBeVisible();

  //no action header
  await expect(
    editSessionDecisionPage.page.getByRole('columnheader', { name: 'Action' }),
  ).not.toBeVisible();

  const timeCell = editSessionDecisionPage.page.getByRole('cell', {
    name: '10:00 - 17:00',
  });
  const serviceCell = editSessionDecisionPage.page.getByRole('cell', {
    name: 'RSV (Adult)',
  });
  const bookedCell = editSessionDecisionPage.page.getByRole('cell', {
    name: '0 booked',
  });
  const unbookedCell = editSessionDecisionPage.page.getByRole('cell', {
    name: '420 unbooked',
  });

  await expect(timeCell).toBeVisible();
  await expect(serviceCell).toBeVisible();
  await expect(bookedCell).toBeVisible();
  await expect(unbookedCell).toBeVisible();
});

test('Clicking into the change UTC session, and clicking through to the edit session page', async ({
  page,
}) => {
  await viewWeekAvailabilityPage.changeButtons[1].click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
  );

  await expect(editSessionDecisionPage.changeHeader).toHaveText(
    'Church Lane PharmacyChange availability for 26 October 2025',
  );

  await editSessionDecisionPage.editLengthCapacityRadioOption.click();
  await editSessionDecisionPage.continueButton.click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/availability/edit?session**`,
  );

  await expect(editSessionPage.editSessionHeader).toHaveText(
    'Edit sessionEdit time and capacity for 26 October 2025',
  );

  await expect(
    editSessionPage.page.getByText('0 booked appointments in this session.'),
  ).toBeVisible();
  await expect(
    editSessionPage.page.getByText(
      '420 unbooked appointments in this session.',
    ),
  ).toBeVisible();

  await expect(editSessionPage.startHourInput).toHaveValue('10');
  await expect(editSessionPage.startMinuteInput).toHaveValue('00');
  await expect(editSessionPage.endHourInput).toHaveValue('17');
  await expect(editSessionPage.endMinuteInput).toHaveValue('00');
});

test('Clicking into the change UTC session, and clicking through to the cancel session page', async ({
  page,
}) => {
  await viewWeekAvailabilityPage.changeButtons[1].click();

  await page.waitForURL(
    `manage-your-appointments/site/${site.id}/view-availability/week/edit-session?date=2025-10-26&session**`,
  );

  await expect(editSessionDecisionPage.changeHeader).toHaveText(
    'Church Lane PharmacyChange availability for 26 October 2025',
  );

  await editSessionDecisionPage.cancelRadioOption.click();
  await editSessionDecisionPage.continueButton.click();

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
    name: '0 booked',
  });
  const unbookedCell = cancelSessionPage.page.getByRole('cell', {
    name: '420 unbooked',
  });

  await expect(timeCell).toBeVisible();
  await expect(serviceCell).toBeVisible();
  await expect(bookedCell).toBeVisible();
  await expect(unbookedCell).toBeVisible();
});
