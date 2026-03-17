import { test, expect } from '../fixtures-v2';
import { daysFromToday } from '../utils/date-utility';

test('Bookings can be setup for the test-scoped site', async ({
  page,
  setup,
}) => {
  const day = daysFromToday(0);
  await setup({
    bookings: [
      {
        fromDate: day,
        fromTime: '09:00:00',
        durationMins: 5,
        service: 'COVID_FLU:65+',
        status: 'Booked',
        availabilityStatus: 'Orphaned',
      },
      {
        fromDate: day,
        fromTime: '09:10:00',
        durationMins: 5,
        service: 'COVID_FLU:65+',
        status: 'Booked',
        availabilityStatus: 'Orphaned',
      },
      {
        fromDate: day,
        fromTime: '17:00:00',
        durationMins: 10,
        service: 'COVID:5_11',
        status: 'Cancelled',
        availabilityStatus: 'Unknown',
      },
    ],
  }).then(async ({ site }) => {
    await page.goto(
      `/manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=${day}&page=1`,
    );
    await page.waitForURL(
      `/manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=${day}&page=1`,
    );

    await expect(page.getByText('09:00')).toBeVisible();
    await expect(page.getByText('09:10')).toBeVisible();
    await expect(page.getByText('17:00')).not.toBeVisible();

    await page.goto(
      `/manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=${day}&page=1&tab=1`,
    );
    await page.waitForURL(
      `/manage-your-appointments/site/${site.id}/view-availability/daily-appointments?date=${day}&page=1&tab=1`,
    );

    await expect(page.getByText('09:00')).not.toBeVisible();
    await expect(page.getByText('09:10')).not.toBeVisible();
    await expect(page.getByText('17:00')).toBeVisible();
  });
});
