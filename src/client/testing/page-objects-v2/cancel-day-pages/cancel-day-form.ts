import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';

export default class CancelDayForm extends MYALayout {
  title = this.page.getByRole('heading');

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

  async verifyHeadingDisplayed(requiredDate: string) {
    const heading = this.title.filter({ hasText: `Cancel ${requiredDate}` });
    await expect(heading).toBeVisible();
  }
}
