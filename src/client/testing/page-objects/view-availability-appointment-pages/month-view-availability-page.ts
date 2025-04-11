import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

type WeekOverview = {
  header: string;
  services: ServiceOverview[];
  totalAppointments: number;
  booked: number;
  unbooked: number;
};

type ServiceOverview = {
  serviceName: string;
  bookedAppointments: number;
};

export default class MonthViewAvailabilityPage extends RootPage {
  readonly nextButton: Locator;

  constructor(page: Page) {
    super(page);
    this.nextButton = page.getByRole('link', {
      name: 'Next',
    });

    this.nextButton = page.getByRole('link', {
      name: 'Next',
    });
  }

  async verifyViewNextMonthButtonDisplayed() {
    await expect(this.nextButton).toBeEnabled();
  }

  async verifyAllWeekCardInformationDisplayedCorrectly(
    expectedWeekOverviews: WeekOverview[],
  ) {
    expectedWeekOverviews.forEach(async weekOverview => {
      const cardDiv = this.page
        .getByRole('heading', {
          name: weekOverview.header,
        })
        .locator('..');

      const header = cardDiv.getByRole('heading', {
        name: weekOverview.header,
      });

      //assert header
      await expect(header).toBeVisible();

      if (weekOverview.services.length > 0) {
        //assert no availability not visible
        expect(cardDiv.getByText('No availability')).not.toBeVisible();

        //only do for a single service for now!!
        const serviceCell = cardDiv.getByRole('cell', {
          name: weekOverview.services[0].serviceName,
        });
        const bookedAppointmentsCell = cardDiv.getByRole('cell', {
          name: weekOverview.services[0].bookedAppointments.toString(),
        });

        await expect(serviceCell).toBeVisible();
        await expect(bookedAppointmentsCell).toBeVisible();

        //totals
        await expect(
          cardDiv.getByText(
            `Total appointments: ${weekOverview.totalAppointments}`,
          ),
        ).toBeVisible();
        await expect(
          cardDiv.getByText(`Booked: ${weekOverview.booked}`, {
            exact: true,
          }),
        ).toBeVisible();
        await expect(
          cardDiv.getByText(`Unbooked: ${weekOverview.unbooked}`, {
            exact: true,
          }),
        ).toBeVisible();
      } else {
        //no services = no availability
        await expect(cardDiv.getByText('No availability')).toBeVisible();
        //totals
        await expect(cardDiv.getByText('Total appointments: 0')).toBeVisible();
        await expect(
          cardDiv.getByText('Booked: 0', { exact: true }),
        ).toBeVisible();
        await expect(
          cardDiv.getByText('Unbooked: 0', { exact: true }),
        ).toBeVisible();
      }
    });
  }

  async verifyViewMonthDisplayed(requiredWeek: string) {
    await this.verifyViewNextMonthButtonDisplayed();
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ has: this.page.getByText(`${requiredWeek}`) }),
    ).toBeVisible();
  }

  async openWeekViewHavingDate(requiredWeek: string) {
    await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${requiredWeek}`) })
      .getByRole('link', { name: 'View week' })
      .click();
  }

  async navigateToRequiredMonth(siteId: string, month: string) {
    await this.page.goto(
      `/manage-your-appointments/site/${siteId}/view-availability?date=${month}`,
    );
  }
}
