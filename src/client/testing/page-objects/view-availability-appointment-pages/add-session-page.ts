import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class AddSessionPage extends RootPage {
  readonly addSessionHeader: Locator;
  readonly goBackButton: Locator;
  readonly continueButton: Locator;
  readonly startTimeHour: Locator;
  readonly startTimeMinute: Locator;
  readonly endTimeHour: Locator;
  readonly endTimeMinute: Locator;
  readonly capacity: Locator;
  readonly duration: Locator;

  constructor(page: Page) {
    super(page);
    this.addSessionHeader = page.getByRole('heading', { level: 1 }).first();
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
    this.startTimeHour = page.getByLabel('Session start time - hour');
    this.startTimeMinute = page.getByLabel('Session start time - minute');
    this.endTimeHour = page.getByLabel('Session end time - hour');
    this.endTimeMinute = page.getByLabel('Session end time - minute');
    this.capacity = page.getByLabel(
      'How many vaccinators or vaccination spaces do you have?',
    );
    this.duration = page.getByLabel('How long are your appointments?');
  }

  async verifyAddSessionPageDisplayed() {
    await expect(this.goBackButton).toBeVisible();
  }

  async addSession(
    startTimeHour: string,
    startTimeMinute: string,
    endTimeHour: string,
    endTimeMinute: string,
    capacity: string,
    duration: string,
  ) {
    await this.startTimeHour.fill(startTimeHour);
    await this.startTimeMinute.fill(startTimeMinute);
    await this.endTimeHour.fill(endTimeHour);
    await this.endTimeMinute.fill(endTimeMinute);
    await this.capacity.fill(capacity);
    await this.duration.fill(duration);
    await this.continueButton.click();
  }

  async updateSessionEndTime(endTimeHour: string, endTimeMinute: string) {
    await this.endTimeHour.fill(endTimeHour);
    await this.endTimeMinute.fill(endTimeMinute);
    await this.continueButton.click();
  }
}
