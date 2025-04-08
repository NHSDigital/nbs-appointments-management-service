import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export type dayOverview = {
  header: string;
  services: serviceOverview[];
  totalAppointments: number;
  booked: number;
  unbooked: number;
};

export type serviceOverview = {
  sessionTimeInterval: string;
  serviceName: string;
  booked: number;
  unbooked: number;
};

export default class WeekViewAvailabilityPage extends RootPage {
  readonly nextButton: Locator;
  readonly previousButton: Locator;
  readonly backToMonthButton: Locator;
  readonly sessionSuccessMsg: Locator;
  readonly viewDailyAppointmentButton: Locator;

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
    this.viewDailyAppointmentButton = page.getByRole('link', {
      name: 'View daily appointments',
    });
  }

  async verifyViewNextWeekButtonDisplayed() {
    await expect(this.nextButton).toBeEnabled();
  }

  async verifyAllDayCardInformationDisplayedCorrectly(
    expectedDayOverviews: dayOverview[],
  ) {
    for (let i = 0; i < expectedDayOverviews.length; i++) {
      const dayOverview = expectedDayOverviews[i];

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

      if (dayOverview.services.length > 0) {
        //assert no availability not visible
        expect(cardDiv.getByText('No availability')).not.toBeVisible();

        //single table
        await expect(cardDiv.locator('table')).toHaveCount(1);

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
        await expect(cardDiv.getByText('No availability')).toBeVisible();

        //no table
        await expect(cardDiv.locator('table')).toHaveCount(0);

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
  }

  async verifyWeekViewDisplayed(requiredDate: string) {
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

  async verifySessionRecordDetail(
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

  async openDailyAppoitmentPage(appointmentDate: string) {
    await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${appointmentDate}`) })
      .getByRole('link', { name: 'View daily appointments' })
      .click();
  }
}
