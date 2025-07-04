import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';
import { DayOverview } from '../../availability';

export default class WeekViewAvailabilityPage extends RootPage {
  readonly nextButton: Locator;
  readonly previousButton: Locator;
  readonly backToMonthButton: Locator;
  readonly sessionSuccessMsg: Locator;

  constructor(page: Page) {
    super(page);

    this.nextButton = page.getByRole('link', {
      name: 'Next',
    });
    this.previousButton = page.getByRole('link', {
      name: 'Previous',
    });
    this.backToMonthButton = page.getByRole('link', {
      name: 'Back to month view',
    });
    this.sessionSuccessMsg = page.getByText(
      'You have successfully created availability for the current site.',
    );
  }

  async verifyViewNextWeekButtonDisplayed() {
    await expect(this.nextButton).toBeEnabled();
  }

  async verifyViewNextAndPreviousWeeksButtonsDisplayed(
    previousWeekText: string,
    nextWeekText: string,
  ) {
    await expect(this.previousButton).toBeVisible();
    await expect(this.previousButton).toBeEnabled();
    await expect(this.previousButton).toContainText(previousWeekText);

    await expect(this.nextButton).toBeVisible();
    await expect(this.nextButton).toBeEnabled();
    await expect(this.nextButton).toContainText(nextWeekText);
  }

  async verifyAllDayCardInformationDisplayedCorrectly(
    dayOverview: DayOverview,
  ) {
    const cardDiv = this.page
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

    if (dayOverview.sessions.length > 0) {
      //assert no availability not visible
      expect(cardDiv.getByText('No availability')).not.toBeVisible();

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
      const singleService = dayOverview.sessions[0];

      const timeCell = cardDiv.getByRole('cell', {
        name: singleService.sessionTimeInterval,
      });
      const serviceCell = cardDiv.getByRole('cell', {
        name: singleService.serviceName,
      });
      const bookedCell = cardDiv.getByRole('cell', {
        name: `${singleService.booked}`,
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
      await expect(cardDiv.getByText('No availability')).toBeVisible();

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
      await expect(cardDiv.getByText('Total appointments: 0')).toBeVisible();
      await expect(
        cardDiv.getByText('Booked: 0', { exact: true }),
      ).toBeVisible();
      await expect(
        cardDiv.getByText('Unbooked: 0', { exact: true }),
      ).toBeVisible();
    }
  }

  async verifySessionDataDisplayedInTheCorrectOrder(dayOverview: DayOverview) {
    const cardDiv = this.page
      .getByRole('heading', {
        name: dayOverview.header,
      })
      .locator('..');

    const sessionTable = cardDiv.getByRole('table');

    //single table
    await expect(sessionTable).toBeVisible();

    const allTableRows = await sessionTable.getByRole('row').all();

    //assert the session data is formed in the correct order and with the right data
    for (let index = 0; index < dayOverview.sessions.length; index++) {
      const expectedSession = dayOverview.sessions[index];

      //start at 1 to ignore table header row
      const tableRow = allTableRows[index + 1];
      const allCells = await tableRow.getByRole('cell').all();

      await expect(allCells[0]).toContainText(
        expectedSession.sessionTimeInterval,
      );
      await expect(allCells[1]).toContainText(expectedSession.serviceName);
      await expect(allCells[2]).toContainText(expectedSession.booked);
      await expect(allCells[3]).toContainText(
        `${expectedSession.unbooked} unbooked`,
      );
    }

    //totals
    if (dayOverview.orphaned === 0) {
      await expect(cardDiv.getByText('manual cancellation')).not.toBeVisible();
    }

    if (dayOverview.orphaned === 1) {
      await expect(
        cardDiv.getByText('There is 1 manual cancellation on this day'),
      ).toBeVisible();
    }

    if (dayOverview.orphaned > 1) {
      await expect(
        cardDiv.getByText(
          `There are ${dayOverview.orphaned} manual cancellations on this day.`,
        ),
      ).toBeVisible();
    }

    await expect(
      cardDiv.getByText(`Total appointments: ${dayOverview.totalAppointments}`),
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
  }

  async verifyDateCardDisplayed(requiredDate: string) {
    await this.verifyViewNextWeekButtonDisplayed();
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredDate}`) }),
    ).toBeVisible();
  }

  async addAvailability(requiredDate: string) {
    const addAvailabilityButton = this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${requiredDate}`) })
      .getByRole('link', { name: 'Add availability to this day' });
    const count: number = await addAvailabilityButton.count();
    if (count == 1) {
      await addAvailabilityButton.click();
    } else {
      await this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredDate}`) })
        .getByRole('link', { name: 'Add Session' })
        .click();
    }
  }

  async verifySessionAdded() {
    await expect(this.sessionSuccessMsg).toBeVisible();
  }

  async verifyAddAvailabilityButtonDisplayed(requiredDate: string) {
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredDate}`) })
        .getByRole('link', { name: 'Add availability to this day' }),
    ).toBeVisible();
  }

  async openChangeAvailabilityPage(requiredDate: string) {
    await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${requiredDate}`) })
      .getByRole('link', { name: 'Change' })
      .first()
      .click();
  }

  async verifyFirstSessionRecordDetail(
    requiredDate: string,
    time: string,
    service: string,
  ) {
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredDate}`) })
        .getByText(`${time}`)
        .first(),
    ).toBeVisible();
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredDate}`) })
        .getByText(`${service}`)
        .first(),
    ).toBeVisible();
  }

  async openDailyAppointmentPage(appointmentDate: string) {
    await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${appointmentDate}`) })
      .getByRole('link', { name: 'View daily appointments' })
      .click();
  }
}
