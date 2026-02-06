import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';
import { parseToUkDatetime } from '@services/timeService';
import { daysFromToday, weekHeaderText } from '../../utils/date-utility';

export default class CancelDayForm extends MYALayout {
  private get headingText(): string {
    const dayIncrement = 29;
    const date = daysFromToday(dayIncrement);
    const requiredDate = daysFromToday(dayIncrement, 'dddd D MMMM');
    const requiredWeekRange = weekHeaderText(date);

    return `Cancel ${requiredDate}`;
  }

  readonly title = this.page.getByRole('heading', {
    name: this.headingText,
  });

  readonly cancelDayRadio = this.page.getByRole('radio', {
    name: 'Yes, I want to cancel the appointments',
  });

  readonly dontCancelDayRadio = this.page.getByRole('radio', {
    name: /^No, I don't want to cancel the appointments/,
  });

  readonly continueButton = this.page.getByRole('button', {
    name: 'Continue',
  });

  readonly cancelDayButton = this.page.getByRole('button', {
    name: 'Cancel day',
  });

  readonly goBackLink = this.page.getByRole('link', {
    name: 'No, go back',
  });

  async verifyHeadingDisplayed(date: string) {
    const parsedDate = parseToUkDatetime(date).format('dddd D MMMM');
    const heading = this.page.getByRole('heading', {
      name: `Cancel ${parsedDate}`,
    });

    await expect(heading).toBeVisible();
  }
}
