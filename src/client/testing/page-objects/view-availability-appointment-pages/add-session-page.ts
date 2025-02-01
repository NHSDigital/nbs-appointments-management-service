import { type Locator, type Page, expect } from '@playwright/test';
import RootPage from '../root';

export default class AddSessionPage extends RootPage {
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
    endTimeHour: string,
    capacity: string,
    duration: string,
  ) {
    await this.startTimeHour.fill(startTimeHour);
    await this.startTimeMinute.fill('00');
    await this.endTimeHour.fill(endTimeHour);
    await this.endTimeMinute.fill('00');
    await this.capacity.fill(capacity);
    await this.duration.fill(duration);
    await this.continueButton.click();
  }
}
