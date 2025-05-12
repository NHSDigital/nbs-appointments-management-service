import { WeekViewPage } from '@testing-page-objects';
import { expect } from '../../fixtures';
import { DayOverview } from './view-week-availability.data';

async function verifyAllDayCardInformationDisplayedCorrectly(
  put: WeekViewPage,
  expectedDayOverviews: DayOverview[],
) {
  expectedDayOverviews.forEach(async dayOverview => {
    const cardDiv = put.page
      .getByRole('heading', {
        name: dayOverview.header,
      })
      .locator('..');

    const header = cardDiv.getByRole('heading', {
      name: dayOverview.header,
    });

    //assert header
    await expect(header).toBeVisible();

    const viewDailyAppointmentsButton = cardDiv.getByRole('link', {
      name: 'View daily appointments',
    });

    //view daily appointments link visible only if any bookings
    if (dayOverview.booked > 0) {
      await expect(viewDailyAppointmentsButton).toBeVisible();
    } else {
      await expect(viewDailyAppointmentsButton).not.toBeVisible();
    }

    if (dayOverview.services.length > 0) {
      //assert no availability not visible
      expect(
        cardDiv.getByText('No availability', { exact: true }),
      ).not.toBeVisible();

      //single table
      expect(cardDiv.getByRole('table')).toBeVisible();

      //table headers!
      await expect(
        cardDiv.getByRole('columnheader', { name: 'Time' }),
      ).toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Services' }),
      ).toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Booked', exact: true }),
      ).toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Unbooked', exact: true }),
      ).toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Action' }),
      ).toBeVisible();

      //only do for a single service for now!!
      const singleService = dayOverview.services[0];

      const timeCell = cardDiv.getByRole('cell', {
        name: singleService.sessionTimeInterval,
      });
      const serviceCell = cardDiv.getByRole('cell', {
        name: singleService.serviceName,
      });
      const bookedCell = cardDiv.getByRole('cell', {
        name: `${singleService.booked} booked`,
      });
      const unbookedCell = cardDiv.getByRole('cell', {
        name: `${singleService.unbooked} unbooked`,
      });

      await expect(timeCell).toBeVisible();
      await expect(serviceCell).toBeVisible();
      await expect(bookedCell).toBeVisible();
      await expect(unbookedCell).toBeVisible();

      //totals
      await expect(
        cardDiv.getByText(
          `Total appointments: ${dayOverview.totalAppointments}`,
        ),
      ).toBeVisible();
      await expect(
        cardDiv.getByText(`Booked: ${dayOverview.booked}`, {
          exact: true,
        }),
      ).toBeVisible();
      await expect(
        cardDiv.getByText(`Unbooked: ${dayOverview.unbooked}`, {
          exact: true,
        }),
      ).toBeVisible();
    } else {
      //no services = no availability
      await expect(
        cardDiv.getByText('No availability', { exact: true }),
      ).toBeVisible();

      //no table
      expect(cardDiv.getByRole('table')).not.toBeVisible();

      //no table headers!
      await expect(
        cardDiv.getByRole('columnheader', { name: 'Time' }),
      ).not.toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Services' }),
      ).not.toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Booked', exact: true }),
      ).not.toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Unbooked', exact: true }),
      ).not.toBeVisible();

      await expect(
        cardDiv.getByRole('columnheader', { name: 'Action' }),
      ).not.toBeVisible();

      //totals
      await expect(
        cardDiv.getByText('Total appointments: 0', { exact: true }),
      ).toBeVisible();
      await expect(
        cardDiv.getByText('Booked: 0', { exact: true }),
      ).toBeVisible();
      await expect(
        cardDiv.getByText('Unbooked: 0', { exact: true }),
      ).toBeVisible();
    }
  });
}
