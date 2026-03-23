import {
  OAuthLoginPage,
  RootPage,
  NotFoundPage,
  AddSessionPage,
  CheckSessionDetailsPage,
  AddServicesPage,
} from '@testing-page-objects';
import { Site } from '@types';
import { test, expect, overrideFeatureFlag, Page } from '../../fixtures';
import {
  daysFromToday,
  getDateInFuture,
  getLongDayDateText,
} from '../../utils/date-utility';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let addSessionPage: AddSessionPage;
let addServicesPage: AddServicesPage;
let checkSessionDetailsPage: CheckSessionDetailsPage;

let site: Site;

test.describe.configure({ mode: 'serial' });

const createSessionOnDay = async (page: Page, dayIncrement: number) => {
  const dateStringForTest = daysFromToday(dayIncrement);

  //create one session
  await page.goto(
    `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${dateStringForTest}`,
  );
  await page.waitForURL(
    `/manage-your-appointments/site/${site.id}/create-availability/wizard?date=${dateStringForTest}`,
  );

  await addSessionPage.addSession('09', '00', '17', '00', '2', '5');
  await addServicesPage.addService('Flu 18 to 64');
  await checkSessionDetailsPage.saveSession();
  await page.waitForURL('**/site/**/view-availability/week?date=**');
};

[true, false].forEach(CancelADateRangeFlagEnabled => {
  test.describe(`Test with CancelADateRangeFlag: '${CancelADateRangeFlagEnabled}'`, () => {
    test.beforeAll(async () => {
      await overrideFeatureFlag(
        'CancelADateRange',
        CancelADateRangeFlagEnabled,
      );
    });

    test.afterAll(async () => {
      await overrideFeatureFlag('CancelADateRange', false);
    });

    test.describe('Cancellation Confirmation', () => {
      test.beforeEach(async ({ page, getTestSite }) => {
        site = getTestSite(2);
        rootPage = new RootPage(page);
        oAuthPage = new OAuthLoginPage(page);
        addSessionPage = new AddSessionPage(page);
        addServicesPage = new AddServicesPage(page);
        checkSessionDetailsPage = new CheckSessionDetailsPage(page);

        await rootPage.goto();
        await rootPage.pageContentLogInButton.click();
        await oAuthPage.signIn();

        await page.goto('/manage-your-appointments/sites');
        await page.waitForURL(`/manage-your-appointments/sites`);
        await page
          .getByRole('link', { name: 'View Church Lane Pharmacy' })
          .click();
        await page.waitForURL(`/manage-your-appointments/site/${site.id}`);
      });

      test('Cancel sessions, verify sessions have been cancelled', async ({
        page,
      }) => {
        const notFoundPage = new NotFoundPage(page);
        if (!CancelADateRangeFlagEnabled) {
          await page.goto(
            `/manage-your-appointments/site/${site.id}/change-availability`,
          );
          await expect(notFoundPage.title).toBeVisible();
          return;
        }

        const fromDayIncrement = 170;
        const toDayIncrement = 171;
        const fromDate = getDateInFuture(fromDayIncrement);
        const toDate = getDateInFuture(toDayIncrement);
        await createSessionOnDay(page, fromDayIncrement);
        await createSessionOnDay(page, toDayIncrement);

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.locator('#start-date-day').fill(fromDate.day.toString());
        await page.locator('#start-date-month').fill(fromDate.month.toString());
        await page.locator('#start-date-year').fill(fromDate.year.toString());
        await page.locator('#end-date-day').fill(toDate.day.toString());
        await page.locator('#end-date-month').fill(toDate.month.toString());
        await page.locator('#end-date-year').fill(toDate.year.toString());
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: 'You are about to cancel',
          }),
        ).toBeVisible();

        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await page
          .getByRole('button', { name: 'Cancel sessions', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: /2 sessions cancelled/i,
          }),
        ).toBeVisible();

        await page
          .getByRole('link', { name: 'Go back to view availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );

        await expect(
          page.getByRole('heading', { name: /View availability/i }),
        ).toBeVisible();

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${fromDate.year}-${fromDate.month}-${fromDate.day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${fromDate.year}-${fromDate.month}-${fromDate.day}`,
        );

        const appointmentCard = page
          .locator('.nhsuk-card')
          .filter({ hasText: getLongDayDateText(fromDate) });

        await expect(appointmentCard).toBeVisible();
        const addLink = appointmentCard.getByRole('link', {
          name: 'Add availability to this day',
        });
        await expect(addLink).toBeVisible();

        expect(appointmentCard.getByText('No availability')).toBeVisible();
        await expect(
          appointmentCard.getByText('Total appointments: 0'),
        ).toBeVisible();

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${toDate.year}-${toDate.month}-${toDate.day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${toDate.year}-${toDate.month}-${toDate.day}`,
        );

        const toDateAppointmentCard = page
          .locator('.nhsuk-card')
          .filter({ hasText: getLongDayDateText(toDate) });

        await expect(toDateAppointmentCard).toBeVisible();
        await expect(
          toDateAppointmentCard.getByRole('link', {
            name: 'Add availability to this day',
          }),
        ).toBeVisible();

        expect(
          toDateAppointmentCard.getByText('No availability'),
        ).toBeVisible();
        await expect(
          toDateAppointmentCard.getByText('Total appointments: 0'),
        ).toBeVisible();
      });

      test('Cancel session, verify session has been cancelled', async ({
        page,
      }) => {
        const notFoundPage = new NotFoundPage(page);
        if (!CancelADateRangeFlagEnabled) {
          await page.goto(
            `/manage-your-appointments/site/${site.id}/change-availability`,
          );
          await expect(notFoundPage.title).toBeVisible();
          return;
        }

        const dayIncrement = 170;
        const fromDate = getDateInFuture(dayIncrement);
        const toDate = getDateInFuture(dayIncrement);
        await createSessionOnDay(page, dayIncrement);

        await page.getByRole('button', { name: 'Change availability' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.getByRole('button', { name: 'Continue to cancel' }).click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );
        await page.locator('#start-date-day').fill(fromDate.day.toString());
        await page.locator('#start-date-month').fill(fromDate.month.toString());
        await page.locator('#start-date-year').fill(fromDate.year.toString());
        await page.locator('#end-date-day').fill(toDate.day.toString());
        await page.locator('#end-date-month').fill(toDate.month.toString());
        await page.locator('#end-date-year').fill(toDate.year.toString());
        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: 'You are about to cancel',
          }),
        ).toBeVisible();

        await page
          .getByRole('button', { name: 'Continue', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await page
          .getByRole('button', { name: 'Cancel sessions', exact: true })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/change-availability`,
        );

        await expect(
          page.getByRole('heading', {
            name: /1 session cancelled/i,
          }),
        ).toBeVisible();

        await page
          .getByRole('link', { name: 'Go back to view availability' })
          .click();
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability`,
        );

        await expect(
          page.getByRole('heading', { name: /View availability/i }),
        ).toBeVisible();

        await page.goto(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${fromDate.year}-${fromDate.month}-${fromDate.day}`,
        );
        await page.waitForURL(
          `/manage-your-appointments/site/${site.id}/view-availability/week?date=${toDate.year}-${toDate.month}-${toDate.day}`,
        );

        const appointmentCard = page
          .locator('.nhsuk-card')
          .filter({ hasText: getLongDayDateText(fromDate) });

        await expect(appointmentCard).toBeVisible();
        const addLink = appointmentCard.getByRole('link', {
          name: 'Add availability to this day',
        });
        await expect(addLink).toBeVisible();

        expect(appointmentCard.getByText('No availability')).toBeVisible();
        await expect(
          appointmentCard.getByText('Total appointments: 0'),
        ).toBeVisible();
      });
    });
  });
});
