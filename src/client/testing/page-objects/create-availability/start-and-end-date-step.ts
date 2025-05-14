import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';

export default class StartAndEndDateStep extends CreateAvailabilityStep {
  readonly repeatingSessionTitle: Locator;
  readonly singleSessionTitle: Locator;

  readonly singleSessionDateDayInput: Locator;
  readonly singleSessionDateMonthInput: Locator;
  readonly singleSessionDateYearInput: Locator;

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

    this.singleSessionDateDayInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Day',
      });
    this.singleSessionDateMonthInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Month',
      });
    this.singleSessionDateYearInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Year',
      });

    this.startDateDayInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Day',
      });
    this.startDateMonthInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Month',
      });
    this.startDateYearInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Year',
      });

    this.endDateDayInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Day',
      });
    this.endDateMonthInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Month',
      });
    this.endDateYearInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Year',
      });
  }
}
