import { Locator, Page } from '@playwright/test';
import { RootPage } from '@testing-page-objects';
import { expect } from '../../fixtures';
import { parseToUkDatetime } from '@services/timeService';

export default class CancelDayForm extends RootPage {
  readonly cancelDayRadio: Locator;
  readonly dontCancelDayRadio: Locator;
  readonly continueButton: Locator;
  readonly cancelDayButton: Locator;
  readonly goBackLink: Locator;

  constructor(page: Page) {
    super(page);

    this.cancelDayRadio = page.getByRole('radio', {
      name: 'Yes, I want to cancel the appointments',
    });
    this.dontCancelDayRadio = page.getByRole('radio', {
      name: /^No, I don't want to cancel the appointments/,
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
    this.cancelDayButton = page.getByRole('button', {
      name: 'Cancel day',
    });
    this.goBackLink = page.getByRole('link', {
      name: 'No, go back',
    });
  }

  async verifyHeadingDisplayed(date: string) {
    const parsedDate = parseToUkDatetime(date).format('dddd D MMMM');
    const heading = this.page.getByRole('heading', {
      name: `Cancel ${parsedDate}`,
    });

    await expect(heading).toBeVisible();
  }
}
