import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';

export default class AddSessionPage extends MYALayout {
  title = this.page.getByRole('heading');

  readonly addSessionHeader = this.page.getByRole('heading', { level: 1 }).first();

  readonly goBackButton = this.page.getByRole('link', {
    name: 'Go back',
  });

  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue',
  });

  readonly startTimeHour = this.page.getByLabel('Session start time - hour');

  readonly startTimeMinute = this.page.getByLabel('Session start time - minute');

  readonly endTimeHour = this.page.getByLabel('Session end time - hour');

  readonly endTimeMinute = this.page.getByLabel('Session end time - minute');

  readonly capacity = this.page.getByLabel(
    'How many vaccinators or vaccination spaces do you have?',
  );

  readonly duration = this.page.getByLabel('How long are your appointments?');

  async verifyHeadingDisplayed(requiredWeekRange: string) {
    const heading = this.title.filter({ hasText: requiredWeekRange });
    await expect(heading).toBeVisible();
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
