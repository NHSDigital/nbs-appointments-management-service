import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

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

  async verifyWeekViewDisplayed() {
    await expect(this.backToMonthButton).toBeVisible();
  }

  async addAvailability(requiredDate: string) {
    const addAvailabilityButton = await this.page
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

  async openDailyAppoitmentPage(appoitmentDate: string) {
    await this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(`${appoitmentDate}`) })
      .getByRole('link', { name: 'View daily appointments' })
      .click();
  }
}
