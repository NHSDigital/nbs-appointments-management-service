//import { RootPage } from '@testing-page-objects';
import { MYALayout } from '@e2etests/types';
import { expect } from '../../fixtures-v2';
import { parseToUkDatetime } from '@services/timeService';

export default class CancelDayForm extends MYALayout {
  readonly title = this.page.getByRole('heading', {
    name: this.site?.name,
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
