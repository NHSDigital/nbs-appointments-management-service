import { type Locator, type Page } from '@playwright/test';

export default class SelectDatesStep {
  readonly goBackButton: Locator;

  readonly stepTitle: Locator;
  readonly startDateInput: Locator;
  readonly endDateInput: Locator;

  readonly continueButton: Locator;

  constructor(page: Page) {
    this.stepTitle = page.getByRole('heading', {
      name: 'Select the dates and create a report',
    });
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });

    this.startDateInput = page.getByLabel('Start date');
    this.endDateInput = page.getByLabel('End date');

    this.continueButton = page.getByRole('button', {
      name: 'Create report',
    });
  }
}
