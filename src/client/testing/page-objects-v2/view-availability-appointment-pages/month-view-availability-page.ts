import { expect } from '../../fixtures-v2';
import { WeekOverview } from '../../availability';
import ChangeAvailabilityPage from '../change-availability/change-availability-page';
import WeekViewAvailabilityPage from './week-view-availability-page';
import { DateComponents } from '@types';
import BaseViewAvailabilityPage from './base-view-availability-page';

export default class MonthViewAvailabilityPage extends BaseViewAvailabilityPage {
  title = this.page.getByRole('heading');

  readonly nextButton = this.page.getByRole('link', {
    name: 'Next',
  });

  readonly previousButton = this.page.getByRole('link', {
    name: 'Previous',
  });

  readonly changeAvailabilityButton = this.page.getByRole('button', {
    name: 'Change availability',
  });

  readonly weeklyCards = this.page.locator('div.nhsuk-card');

  async clickChangeAvailabilityButton(): Promise<ChangeAvailabilityPage> {
    await this.changeAvailabilityButton.click();

    await this.page.waitForURL(url =>
      url.pathname.includes(
        `/manage-your-appointments/site/${this.site?.id}/change-availability`,
      ),
    );

    return new ChangeAvailabilityPage(this.page, this.site);
  }

  async clickViewWeekInCardByDate(
    dateRange: string,
  ): Promise<WeekViewAvailabilityPage> {
    const targetCard = this.weeklyCards.filter({ hasText: dateRange });
    await targetCard.getByRole('link', { name: 'View week' }).click();

    await this.page.waitForURL(url =>
      url.pathname.includes(`/site/${this.site?.id}/view-availability/week`),
    );

    return new WeekViewAvailabilityPage(this.page, this.site);
  }

  async verifyHeadingDisplayed(requiredMonthYearDate: string) {
    const heading = this.title.filter({
      hasText: `View availability for ${requiredMonthYearDate}`,
    });
    await expect(heading).toBeVisible();
  }

  async verifyViewNextMonthButtonDisplayed() {
    await expect(this.nextButton).toBeEnabled();
  }

  async verifyViewNextAndPreviousMonthButtonsAreDisplayed(
    previousMonthText: string,
    nextMonthText: string,
  ) {
    await expect(this.previousButton).toBeVisible();
    await expect(this.previousButton).toBeEnabled();
    await expect(this.previousButton).toContainText(previousMonthText);

    await expect(this.nextButton).toBeVisible();
    await expect(this.nextButton).toBeEnabled();
    await expect(this.nextButton).toContainText(nextMonthText);
  }

  async verifyAllWeekCardInformationDisplayedCorrectly(
    weekOverview: WeekOverview,
  ) {
    const cardDiv = this.page
      .getByRole('heading', {
        name: weekOverview.header,
      })
      .locator('../..');

    const header = cardDiv.getByRole('heading', {
      name: weekOverview.header,
    });

    //assert header
    await expect(header).toBeVisible();

    if (weekOverview.sessions.length > 0) {
      //assert no availability not visible
      expect(cardDiv.getByText('No availability')).not.toBeVisible();

      //only do for a single service for now!!
      const serviceCell = cardDiv.getByRole('cell', {
        name: weekOverview.sessions[0].serviceName,
      });
      const bookedAppointmentsCell = cardDiv.getByRole('cell', {
        name: weekOverview.sessions[0].bookedAppointments.toString(),
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

  async goToWeekForDate(
    date: DateComponents,
  ): Promise<WeekViewAvailabilityPage> {
    await this.page.goto(
      `/manage-your-appointments/site/${this.site?.id}/view-availability/week?date=${date.year}-${date.month}-${date.day}`,
    );
    await this.page.waitForURL(
      `/manage-your-appointments/site/${this.site?.id}/view-availability/week?date=${date.year}-${date.month}-${date.day}`,
    );

    return new WeekViewAvailabilityPage(this.page, this.site);
  }
}
