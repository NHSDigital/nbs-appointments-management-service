import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';

export default class StartAndEndDateStep extends CreateAvailabilityStep {
  readonly repeatingSessionTitle: Locator;
  readonly singleSessionTitle: Locator;

  readonly singleDateDayInput: Locator;
  readonly singleDateMonthInput: Locator;
  readonly singleDateYearInput: Locator;

  readonly startDateDayInput: Locator;
  readonly startDateMonthInput: Locator;
  readonly startDateYearInput: Locator;

  readonly endDateDayInput: Locator;
  readonly endDateMonthInput: Locator;
  readonly endDateYearInput: Locator;

  constructor(page: Page) {
    super(page);
    this.repeatingSessionTitle = page.getByRole('heading', {
      name: /Add start and end dates/,
    });
    this.singleSessionTitle = page.getByRole('heading', {
      name: /Add a date for your session/,
    });

    this.singleDateDayInput = this.getDateInput('Session date', 'Day');
    this.singleDateMonthInput = this.getDateInput('Session date', 'Month');
    this.singleDateYearInput = this.getDateInput('Session date', 'Year');

    this.startDateDayInput = this.getDateInput('Start date', 'Day');
    this.startDateMonthInput = this.getDateInput('Start date', 'Month');
    this.startDateYearInput = this.getDateInput('Start date', 'Year');

    this.endDateDayInput = this.getDateInput('End date', 'Day');
    this.endDateMonthInput = this.getDateInput('End date', 'Month');
    this.endDateYearInput = this.getDateInput('End date', 'Year');
  }

  private getDateInput(groupLabel: string, inputLabel: string): Locator {
    return this.page
      .getByRole('group')
      .filter({ has: this.page.getByText(groupLabel, { exact: true }) })
      .getByLabel(inputLabel);
  }
}
